using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Yeast.Ion;

namespace Yeast.Json
{
    public struct ToJsonSettings
    {
        public bool prettyPrint;
        public int indentSize;
    }
    public struct FromJsonSettings
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

    public class JsonConverter : IFromConverter<string, IIonValue, FromJsonSettings, JsonConversionException>, IIntoConverter<IIonValue, string, ToJsonSettings, JsonConversionException>
    {
        private FromJsonSettings fromJsonSettings = new();
        private ToJsonSettings toJsonSettings = new();

        public bool TryFrom(string value, out IIonValue result, FromJsonSettings settings, out JsonConversionException exception)
        {
            fromJsonSettings = settings;
            try
            {
                int position = 0;
                result = ParseValue(value, ref position);
                exception = null;
                return true;
            }
            catch (JsonConversionException e)
            {
                result = null;
                exception = e;
                return false;
            }
        }

        public bool TryInto(IIonValue value, out string result, ToJsonSettings settings, out JsonConversionException exception)
        {
            toJsonSettings = settings;
            try
            {
                result = Stringify(value, 0);
                exception = null;
                return true;
            }
            catch (JsonConversionException e)
            {
                result = null;
                exception = e;
                return false;
            }
        }

        private string Stringify(IIonValue value, int indentLevel)
        {
            if (value is StringValue stringValue)
            {
                return JsonStringUtils.EscapeString(stringValue.value);
            }
            else if (value is IntegerValue integerValue)
            {
                return integerValue.value.ToString(CultureInfo.InvariantCulture);
            }
            else if (value is BooleanValue booleanValue)
            {
                return booleanValue.value ? "true" : "false";
            }
            else if (value is FloatValue floatValue)
            {
                return StringifyFloat(floatValue.value);

            }
            else if (value is NullValue)
            {
                return "null";
            }
            else if (value is ArrayValue arrayValue)
            {
                return StringifyArray(arrayValue.value, indentLevel);
            }
            else if (value is MapValue objectValue)
            {
                return StringifyMap(objectValue.value, indentLevel);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private string StringifyFloat(double value)
        {
            if (double.IsNaN(value)) return "NaN";
            else if (double.IsPositiveInfinity(value)) return "+Infinity";
            else if (double.IsNegativeInfinity(value)) return "-Infinity";
            var str = value.ToString("R", CultureInfo.InvariantCulture);
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c != '-' && !char.IsDigit(c)) return str;
            }
            return str + ".0";
        }

        private string StringifyArray(List<IIonValue> values, int indentLevel)
        {
            StringBuilder builder = new();
            builder.Append("[");
            if (toJsonSettings.prettyPrint)
            {
                builder.Append("\n");
                indentLevel += toJsonSettings.indentSize;
                for (int i = 0; i < indentLevel; i++) builder.Append(" ");
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (i >= 1)
                {
                    builder.Append(",");
                    if (toJsonSettings.prettyPrint)
                    {
                        builder.Append("\n");
                        for (int j = 0; j < indentLevel; j++) builder.Append(" ");
                    }
                }
                builder.Append(Stringify(values[i], indentLevel));
            }
            if (toJsonSettings.prettyPrint)
            {
                indentLevel -= toJsonSettings.indentSize;
                builder.Append("\n");
                for (int i = 0; i < indentLevel; i++) builder.Append(" ");
            }
            builder.Append("]");
            return builder.ToString();
        }

        private string StringifyMap(Dictionary<string, IIonValue> values, int indentLevel)
        {
            StringBuilder builder = new();
            builder.Append("{");
            if (toJsonSettings.prettyPrint)
            {
                builder.Append("\n");
                indentLevel += toJsonSettings.indentSize;
                for (int i = 0; i < indentLevel; i++) builder.Append(" ");
            }

            int count = 0;
            foreach (var pair in values)
            {
                if (count >= 1)
                {
                    builder.Append(",");
                    if (toJsonSettings.prettyPrint)
                    {
                        builder.Append("\n");
                        for (int j = 0; j < indentLevel; j++) builder.Append(" ");
                    }
                }
                builder.Append(JsonStringUtils.EscapeString(pair.Key));
                builder.Append(":");
                if (toJsonSettings.prettyPrint) builder.Append(" ");
                builder.Append(Stringify(pair.Value, indentLevel));
                count++;
            }
            if (toJsonSettings.prettyPrint)
            {
                indentLevel -= toJsonSettings.indentSize;
                builder.Append("\n");
                for (int i = 0; i < indentLevel; i++) builder.Append(" ");
            }
            builder.Append("}");
            return builder.ToString();
        }

        private IIonValue ParseValue(string text, ref int position)
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


        private StringValue ParseString(string text, ref int position)
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
            return new StringValue(JsonStringUtils.UnescapeString(value));
        }

        private ArrayValue ParseArray(string text, ref int position)
        {
            ExpectChar('[', text, ref position);
            List<IIonValue> values = new();
            SkipWhitespace(text, ref position);
            while (!IsChar(']', text, ref position))
            {
                if (values.Count >= 1) ExpectChar(',', text, ref position);
                SkipWhitespace(text, ref position);
                values.Add(ParseValue(text, ref position));
                SkipWhitespace(text, ref position);
            }
            AdvancePosition(text, ref position);
            return new ArrayValue(values);
        }

        private MapValue ParseObject(string text, ref int position)
        {
            ExpectChar('{', text, ref position);
            Dictionary<string, IIonValue> values = new();
            SkipWhitespace(text, ref position);
            while (!IsChar('}', text, ref position))
            {
                if (values.Count >= 1) ExpectChar(',', text, ref position);
                SkipWhitespace(text, ref position);
                string key = ParseString(text, ref position).value;
                SkipWhitespace(text, ref position);
                ExpectChar(':', text, ref position);
                SkipWhitespace(text, ref position);
                IIonValue value = ParseValue(text, ref position);
                values.Add(key, value);
                SkipWhitespace(text, ref position);
            }
            AdvancePosition(text, ref position);
            return new MapValue(values);
        }

        private BooleanValue ParseBoolean(string text, ref int position)
        {
            if (IsChar('t', text, ref position))
            {
                ExpectString("true", text, ref position);
                return new BooleanValue(true);
            }
            else
            {
                ExpectString("false", text, ref position);
                return new BooleanValue(false);
            }
        }

        private NullValue ParseNull(string text, ref int position)
        {
            ExpectString("null", text, ref position);
            return new NullValue();
        }

        private IIonValue ParseNumber(string text, ref int position)
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
                return new FloatValue(double.PositiveInfinity);
            }
            else if (value == "-Infinity")
            {
                return new FloatValue(double.NegativeInfinity);
            }
            else if (value == "NaN")
            {
                return new FloatValue(double.NaN);
            }

            try
            {
                if (isFloat)
                {
                    return new FloatValue(double.Parse(value, CultureInfo.InvariantCulture));
                }
                else
                {
                    return new IntegerValue(long.Parse(value, CultureInfo.InvariantCulture));
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

    public static class JsonStringUtils
    {
        public static string EscapeString(string input)
        {
            StringBuilder literal = new(input.Length + 2);
            literal.Append("\"");
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default:
                        // ASCII printable character
                        if (c >= 0x20 && c <= 0x7e)
                        {
                            literal.Append(c);
                            // As UTF16 escaped character
                        }
                        else
                        {
                            literal.Append(@"\u");
                            literal.Append(((int)c).ToString("x4"));
                        }
                        break;
                }
            }
            literal.Append("\"");
            return literal.ToString();
        }

        public static string UnescapeString(string literal)
        {
            StringBuilder result = new();

            int i = 0;
            while (i < literal.Length)
            {
                char c = literal[i];
                if (c == '\\')
                {
                    // Handle escape sequences
                    if (i + 1 >= literal.Length) throw new JsonConversionException($"Unexpected end of string {literal}", i);
                    char nextChar = literal[i + 1];
                    switch (nextChar)
                    {
                        case '\\': result.Append("\\"); break;
                        case 'n': result.Append("\n"); break;
                        case 't': result.Append("\t"); break;
                        case 'r': result.Append("\r"); break;
                        case 'b': result.Append("\b"); break;
                        case 'f': result.Append("\f"); break;
                        case 'a': result.Append("\a"); break;
                        case 'v': result.Append("\v"); break;
                        case '0': result.Append("\0"); break;
                        case '\"': result.Append("\""); break;
                        case 'u':
                            // Parse Unicode escape sequence
                            string unicodeValue = literal.Substring(i + 2, 4);
                            result.Append((char)int.Parse(unicodeValue, NumberStyles.HexNumber));
                            i += 4;
                            break;
                        default:
                            // Unrecognized escape sequence, treat as a literal character
                            result.Append(c);
                            break;
                    }
                    i += 2;
                }
                else
                {
                    // Regular character
                    result.Append(c);
                    i++;
                }
            }

            return result.ToString();
        }
    }
}
