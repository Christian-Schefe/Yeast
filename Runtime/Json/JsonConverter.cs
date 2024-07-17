using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Yeast.Memento;
using Yeast.Utils;

namespace Yeast.Json
{
    public struct JsonSerializationSettings
    {
        public bool prettyPrint;
        public int indentSize;
    }
    public struct JsonDeserializationSettings
    {

    }

    public class JsonConversionException : Exception
    {
        private readonly int position;

        public JsonConversionException(string message, int position) : base(message)
        {
            this.position = position;
        }

        public override string ToString()
        {
            return $"{base.ToString()} at position {position}";
        }
    }

    public class JsonConverter : BaseMementoConverter<string, JsonSerializationSettings, JsonDeserializationSettings>
    {
        protected override IMemento Deserialize(string value)
        {
            int position = 0;
            return ParseValue(value, ref position);
        }

        protected override string Serialize(IMemento value)
        {
            StringBuilder result = new();
            Stringify(result, value, 0);
            return result.ToString();
        }

        private void Stringify(StringBuilder result, IMemento value, int indentLevel)
        {
            if (value is StringMemento stringValue)
            {
                result.Append('"');
                result.Append(StringUtils.EscapeString(stringValue.value));
                result.Append('"');
            }
            else if (value is IntegerMemento integerValue)
            {
                result.Append(StringUtils.LongToString(integerValue.value));
            }
            else if (value is BoolMemento booleanValue)
            {
                result.Append(booleanValue.value ? "true" : "false");
            }
            else if (value is DecimalMemento floatValue)
            {
                StringifyFloat(result, floatValue.value);
            }
            else if (value is NullMemento)
            {
                result.Append("null");
            }
            else if (value is ArrayMemento arrayValue)
            {
                StringifyArray(result, arrayValue.value, indentLevel);
            }
            else if (value is DictMemento objectValue)
            {
                StringifyDict(result, objectValue.value, indentLevel);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void StringifyFloat(StringBuilder result, double value)
        {
            if (double.IsNaN(value)) result.Append("NaN");
            else if (double.IsPositiveInfinity(value)) result.Append("+Infinity");
            else if (double.IsNegativeInfinity(value)) result.Append("-Infinity");
            else result.Append(StringUtils.DoubleToString(value));
        }

        private void StringifyArray(StringBuilder result, IMemento[] values, int indentLevel)
        {
            result.Append("[");
            if (serializationSettings.prettyPrint)
            {
                result.Append("\n");
                indentLevel += serializationSettings.indentSize;
                for (int i = 0; i < indentLevel; i++) result.Append(" ");
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (i >= 1)
                {
                    result.Append(",");
                    if (serializationSettings.prettyPrint)
                    {
                        result.Append("\n");
                        for (int j = 0; j < indentLevel; j++) result.Append(" ");
                    }
                }
                Stringify(result, values[i], indentLevel);
            }
            if (serializationSettings.prettyPrint)
            {
                indentLevel -= serializationSettings.indentSize;
                result.Append("\n");
                for (int i = 0; i < indentLevel; i++) result.Append(" ");
            }
            result.Append("]");
        }

        private void StringifyDict(StringBuilder result, Dictionary<string, IMemento> values, int indentLevel)
        {
            result.Append("{");
            if (serializationSettings.prettyPrint)
            {
                result.Append("\n");
                indentLevel += serializationSettings.indentSize;
                for (int i = 0; i < indentLevel; i++) result.Append(" ");
            }

            int count = 0;
            foreach (var pair in values)
            {
                if (count >= 1)
                {
                    result.Append(",");
                    if (serializationSettings.prettyPrint)
                    {
                        result.Append("\n");
                        for (int j = 0; j < indentLevel; j++) result.Append(" ");
                    }
                }
                result.Append('"');
                result.Append(StringUtils.EscapeString(pair.Key));
                result.Append('"');
                result.Append(':');
                if (serializationSettings.prettyPrint) result.Append(" ");
                Stringify(result, pair.Value, indentLevel);
                count++;
            }
            if (serializationSettings.prettyPrint)
            {
                indentLevel -= serializationSettings.indentSize;
                result.Append("\n");
                for (int i = 0; i < indentLevel; i++) result.Append(" ");
            }
            result.Append("}");
        }

        private IMemento ParseValue(string text, ref int position)
        {
            SkipWhitespace(text, ref position);
            CheckPosition(text, ref position);
            if (text[position] == '"')
            {
                return ParseString(text, ref position);
            }
            else if (text[position] == '[')
            {
                return ParseArray(text, ref position);
            }
            else if (text[position] == '{')
            {
                return ParseObject(text, ref position);
            }
            else if (text[position] == 't' || text[position] == 'f')
            {
                return ParseBoolean(text, ref position);
            }
            else if (text[position] == 'n')
            {
                return ParseNull(text, ref position);
            }
            else
            {
                return ParseNumber(text, ref position);
            }
        }


        private StringMemento ParseString(string text, ref int position)
        {
            ExpectChar('"', text, ref position);
            int start = position;

            while (!IsChar('"', text, ref position))
            {
                if (IsChar('\\', text, ref position))
                {
                    AdvancePosition(text, ref position);
                }
                AdvancePosition(text, ref position);
            }

            string value = text[start..position];
            AdvancePosition(text, ref position);
            return new StringMemento(StringUtils.UnescapeString(value));
        }

        private ArrayMemento ParseArray(string text, ref int position)
        {
            ExpectChar('[', text, ref position);
            List<IMemento> values = new();
            SkipWhitespace(text, ref position);
            while (!IsChar(']', text, ref position))
            {
                if (values.Count >= 1) ExpectChar(',', text, ref position);
                SkipWhitespace(text, ref position);
                values.Add(ParseValue(text, ref position));
                SkipWhitespace(text, ref position);
            }
            AdvancePosition(text, ref position);
            return new ArrayMemento(values);
        }

        private DictMemento ParseObject(string text, ref int position)
        {
            ExpectChar('{', text, ref position);
            Dictionary<string, IMemento> values = new();
            SkipWhitespace(text, ref position);
            while (!IsChar('}', text, ref position))
            {
                if (values.Count >= 1) ExpectChar(',', text, ref position);
                SkipWhitespace(text, ref position);
                string key = ParseString(text, ref position).value;
                SkipWhitespace(text, ref position);
                ExpectChar(':', text, ref position);
                SkipWhitespace(text, ref position);
                IMemento value = ParseValue(text, ref position);
                values.Add(key, value);
                SkipWhitespace(text, ref position);
            }
            AdvancePosition(text, ref position);

            return new DictMemento(values);
        }

        private BoolMemento ParseBoolean(string text, ref int position)
        {
            if (IsChar('t', text, ref position))
            {
                ExpectString("true", text, ref position);
                return new BoolMemento(true);
            }
            else
            {
                ExpectString("false", text, ref position);
                return new BoolMemento(false);
            }
        }

        private NullMemento ParseNull(string text, ref int position)
        {
            ExpectString("null", text, ref position);
            return new NullMemento();
        }

        private IMemento ParseNumber(string text, ref int position)
        {
            int start = position;
            bool isFloat = false;
            CheckPosition(text, ref position);
            char c = text[position];
            while (char.IsLetterOrDigit(c) || c == '.' || c == '+' || c == '-')
            {
                if (c == '.' || char.IsLetter(c))
                {
                    isFloat = true;
                }
                position++;
                if (position >= text.Length) break;
                c = text[position];
            }
            string value = text[start..position];

            if (value == "+Infinity")
            {
                return new DecimalMemento(double.PositiveInfinity);
            }
            else if (value == "-Infinity")
            {
                return new DecimalMemento(double.NegativeInfinity);
            }
            else if (value == "NaN")
            {
                return new DecimalMemento(double.NaN);
            }

            try
            {
                if (isFloat)
                {
                    return new DecimalMemento(StringUtils.StringToDouble(value));
                }
                else
                {
                    return new IntegerMemento(StringUtils.StringToLong(value));
                }
            }
            catch (FormatException e)
            {
                throw new JsonConversionException("Invalid number format: " + value + " " + e.Message, position);
            }
        }

        private bool IsChar(char c, string text, ref int position)
        {
            CheckPosition(text, ref position);
            return text[position] == c;
        }

        private void ExpectChar(char expected, string text, ref int position)
        {
            CheckPosition(text, ref position);
            if (text[position] != expected)
            {
                throw new JsonConversionException($"Expected '{expected}'", position);
            }
            position++;
        }

        private void ExpectString(string expected, string text, ref int position)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                ExpectChar(expected[i], text, ref position);
            }
        }

        private void AdvancePosition(string text, ref int position)
        {
            CheckPosition(text, ref position);
            position++;
        }

        private void CheckPosition(string text, ref int position)
        {
            if (text.Length <= position)
            {
                throw new JsonConversionException($"Unexpected end of file", position);
            }
        }

        private void SkipWhitespace(string text, ref int position)
        {
            CheckPosition(text, ref position);
            while (char.IsWhiteSpace(text[position]))
            {
                AdvancePosition(text, ref position);
            }
        }
    }
}
