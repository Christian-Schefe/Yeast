using System.Collections.Generic;
using System.Text;
using Yeast.Utils;

namespace Yeast.Json
{
    public class JsonParser
    {
        private readonly string json;
        private int position;

        public JsonParser(string json)
        {
            this.json = json;
            position = 0;
        }

        public JsonValue ParseValue()
        {
            SkipWhitespace();
            var c = PeekChar();
            if (c == '"')
            {
                return ParseString();
            }
            else if (c == '[')
            {
                return ParseArray();
            }
            else if (c == '{')
            {
                return ParseObject();
            }
            else if (c == 't' || c == 'f')
            {
                return ParseBool();
            }
            else if (c == 'n')
            {
                return ParseNull();
            }
            else
            {
                return ParseNumber();
            }
        }

        private JsonNumber ParseNumber()
        {
            var sb = new StringBuilder();
            var c = PeekChar();
            while (char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '+')
            {
                sb.Append(c);
                AdvancePosition();

                if (position >= json.Length) break;
                c = PeekChar();
            }
            var str = sb.ToString();
            return new JsonNumber(StringUtils.StringToDouble(str));
        }

        private JsonBoolean ParseBool()
        {
            if (IsChar('t'))
            {
                ConsumeString("true");
                return new JsonBoolean(true);
            }
            else
            {
                ConsumeString("false");
                return new JsonBoolean(false);
            }
        }

        private JsonString ParseString()
        {
            ConsumeChar('"');
            var sb = new StringBuilder();
            while (!IsChar('"'))
            {
                if (IsChar('\\'))
                {
                    sb.Append(json[position]);
                    AdvancePosition();
                }
                sb.Append(json[position]);
                AdvancePosition();
            }
            ConsumeChar('"');
            return new JsonString(StringUtils.UnescapeJsonString(sb.ToString()));
        }

        private JsonArray ParseArray()
        {
            List<JsonValue> list = new();
            ConsumeChar('[');
            SkipWhitespace();
            bool first = true;
            while (!IsChar(']'))
            {
                if (!first) ConsumeChar(',');
                first = false;
                SkipWhitespace();

                list.Add(ParseValue());
                SkipWhitespace();
            }
            ConsumeChar(']');
            return new JsonArray(list);
        }

        private JsonObject ParseObject()
        {
            Dictionary<string, JsonValue> dict = new();
            ConsumeChar('{');
            SkipWhitespace();
            bool first = true;
            while (!IsChar('}'))
            {
                if (!first) ConsumeChar(',');
                first = false;
                SkipWhitespace();

                var key = ParseString().value;
                SkipWhitespace();
                ConsumeChar(':');
                SkipWhitespace();
                var value = ParseValue();
                dict[key] = value;
                SkipWhitespace();
            }
            ConsumeChar('}');
            return new JsonObject(dict);
        }

        private JsonNull ParseNull()
        {
            ConsumeString("null");
            return new JsonNull();
        }

        private char PeekChar()
        {
            CheckPosition();
            return json[position];
        }

        private bool IsChar(char c)
        {
            return PeekChar() == c;
        }

        private void ConsumeChar(char expected)
        {
            if (!IsChar(expected))
            {
                throw new System.Exception($"Expected '{expected}' at {position}, found  {json[position]}");
            }
            position++;
        }

        private void ConsumeString(string expected)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                ConsumeChar(expected[i]);
            }
        }

        private void AdvancePosition()
        {
            CheckPosition();
            position++;
        }

        private void CheckPosition()
        {
            if (json.Length <= position)
            {
                throw new System.Exception($"Unexpected end of json");
            }
        }

        private void SkipWhitespace()
        {
            CheckPosition();
            while (char.IsWhiteSpace(json[position]))
            {
                AdvancePosition();
            }
        }
    }
}
