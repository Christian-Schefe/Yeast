using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using static JsonConverter;

public class JsonConverter : IConverter<string>
{
    public SimpleValue Parse(Type type, string text)
    {
        int position = 0;
        return ParseValue(text, ref position);
    }

    public string Stringify(SimpleValue value)
    {
        if (value is SimpleValue.StringValue stringValue)
        {
            return JsonStringUtils.EscapeString(stringValue.value);
        }
        else if (value is SimpleValue.IntegerValue integerValue)
        {
            return integerValue.value.ToString(CultureInfo.InvariantCulture);
        }
        else if (value is SimpleValue.BooleanValue booleanValue)
        {
            return booleanValue.value ? "true" : "false";
        }
        else if (value is SimpleValue.FloatValue floatValue)
        {
            return floatValue.value.ToString("R", CultureInfo.InvariantCulture);
        }
        else if (value is SimpleValue.NullValue)
        {
            return "null";
        }
        else if (value is SimpleValue.ArrayValue arrayValue)
        {
            StringBuilder builder = new();
            builder.Append("[");
            for (int i = 0; i < arrayValue.value.Count; i++)
            {
                if (i >= 1) builder.Append(",");
                builder.Append(Stringify(arrayValue.value[i]));
            }
            builder.Append("]");
            return builder.ToString();
        }
        else if (value is SimpleValue.MapValue objectValue)
        {
            StringBuilder builder = new();
            builder.Append("{");
            bool first = true;
            foreach (var pair in objectValue.value)
            {
                if (!first) builder.Append(",");
                builder.Append($"\"{pair.Key}\":{Stringify(pair.Value)}");
                first = false;
            }
            builder.Append("}");
            return builder.ToString();
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private SimpleValue ParseValue(string text, ref int position)
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

    private SimpleValue.StringValue ParseString(string text, ref int position)
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
        return new SimpleValue.StringValue { value = JsonStringUtils.UnescapeString(value) };
    }

    private SimpleValue.ArrayValue ParseArray(string text, ref int position)
    {
        ExpectChar('[', text, ref position);
        List<SimpleValue> values = new();
        SkipWhitespace(text, ref position);
        while (!IsChar(']', text, ref position))
        {
            if (values.Count >= 1) ExpectChar(',', text, ref position);
            SkipWhitespace(text, ref position);
            values.Add(ParseValue(text, ref position));
            SkipWhitespace(text, ref position);
        }
        AdvancePosition(text, ref position);
        return new SimpleValue.ArrayValue { value = values };
    }

    private SimpleValue.MapValue ParseObject(string text, ref int position)
    {
        ExpectChar('{', text, ref position);
        Dictionary<string, SimpleValue> values = new();
        SkipWhitespace(text, ref position);
        while (!IsChar('}', text, ref position))
        {
            if (values.Count >= 1) ExpectChar(',', text, ref position);
            SkipWhitespace(text, ref position);
            string key = ParseString(text, ref position).value;
            SkipWhitespace(text, ref position);
            ExpectChar(':', text, ref position);
            SkipWhitespace(text, ref position);
            SimpleValue value = ParseValue(text, ref position);
            values.Add(key, value);
            SkipWhitespace(text, ref position);
        }
        AdvancePosition(text, ref position);
        return new SimpleValue.MapValue { value = values };
    }

    private SimpleValue.BooleanValue ParseBoolean(string text, ref int position)
    {
        if (IsChar('t', text, ref position))
        {
            ExpectString("true", text, ref position);
            return new SimpleValue.BooleanValue { value = true };
        }
        else
        {
            ExpectString("false", text, ref position);
            return new SimpleValue.BooleanValue { value = false };
        }
    }

    private SimpleValue.NullValue ParseNull(string text, ref int position)
    {
        ExpectString("null", text, ref position);
        return new SimpleValue.NullValue();
    }

    private SimpleValue ParseNumber(string text, ref int position)
    {
        int start = position;
        bool isFloat = false;
        CheckPosition(text, ref position);
        char c = text[position];
        while (char.IsDigit(c) || c == '.' || c == '+' || c == '-' || c == 'e' || c == 'E')
        {
            if (c == '.')
            {
                isFloat = true;
            }
            position++;
            if (position >= text.Length) break;
            c = text[position];
        }
        string value = text[start..position];

        try
        {
            if (isFloat)
            {
                return new SimpleValue.FloatValue { value = double.Parse(value, CultureInfo.InvariantCulture) };
            }
            else
            {
                return new SimpleValue.IntegerValue { value = long.Parse(value, CultureInfo.InvariantCulture) };
            }
        }
        catch (FormatException)
        {
            throw new MalformedJsonException("Invalid number format", position);
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
            throw new MalformedJsonException($"Expected '{expected}'", position);
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
            throw new MalformedJsonException($"Unexpected end of file", position);
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

    public class MalformedJsonException : Exception
    {
        private readonly int position;

        public MalformedJsonException(string message, int position) : base(message)
        {
            this.position = position;
        }

        public override string ToString()
        {
            return $"{base.ToString()} at position {position}";
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
        literal = literal.Trim('\"');

        StringBuilder result = new();

        int i = 0;
        while (i < literal.Length)
        {
            char c = literal[i];
            if (c == '\\')
            {
                // Handle escape sequences
                if (i + 1 >= literal.Length) throw new MalformedJsonException("Unexpected end of string", i);
                char nextChar = literal[i + 1];
                switch (nextChar)
                {
                    case '\\': result.Append("\\"); break;
                    case 'n': result.Append("\n"); break;
                    case 't': result.Append("\t"); break;
                    case 'r': result.Append("\r"); break;
                    case 'u':
                        // Parse Unicode escape sequence
                        string unicodeValue = literal.Substring(i + 2, 4);
                        result.Append((char)int.Parse(unicodeValue, System.Globalization.NumberStyles.HexNumber));
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