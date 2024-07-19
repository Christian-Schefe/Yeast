using System.Collections.Generic;
using System.Text;
using Yeast.Utils;

namespace Yeast.Xml
{
    public class XmlParser
    {
        private readonly string xml;
        private int position;

        public XmlParser(string xml)
        {
            this.xml = xml;
            position = 0;
        }

        public XmlValue ParseValue()
        {
            var c = PeekChar();
            if (c == '<')
            {
                return ParseElement();
            }
            else
            {
                return ParseString();
            }
        }

        public XmlString ParseString()
        {
            var sb = new StringBuilder();
            while (!IsChar('<'))
            {
                sb.Append(PeekChar());
                AdvancePosition();

                if (position >= xml.Length)
                {
                    break;
                }
            }
            return new XmlString(StringUtils.UnescapeJsonString(StringUtils.UnescapeXMLString(sb.ToString())));
        }

        public XmlElement ParseElement()
        {
            ConsumeChar('<');
            var name = ParseElementName();
            var attributes = ParseAttributes();
            if (IsChar('/'))
            {
                ConsumeChar('/');
                ConsumeChar('>');
                return new XmlElement(name, attributes, new List<XmlValue>());
            }
            ConsumeChar('>');
            var children = ParseChildren();
            ConsumeClosingTag(name);
            return new XmlElement(name, attributes, children);
        }

        public void ConsumeClosingTag(string tagName)
        {
            ConsumeString("</");
            SkipWhitespace();
            ConsumeString(tagName);
            SkipWhitespace();
            ConsumeChar('>');
        }

        public List<XmlValue> ParseChildren()
        {
            var children = new List<XmlValue>();
            while (!IsString("</"))
            {
                children.Add(ParseValue());
                SkipWhitespace();
            }
            return children;
        }

        public string ParseElementName()
        {
            SkipWhitespace();
            var sb = new StringBuilder();
            while (!IsChar(' ') && !IsChar('>') && !IsChar('/'))
            {
                sb.Append(PeekChar());
                AdvancePosition();
            }
            return sb.ToString();
        }

        public Dictionary<string, string> ParseAttributes()
        {
            var attributes = new Dictionary<string, string>();
            while (!IsChar('>') && !IsChar('/'))
            {
                var name = ParseAttributeName();
                var value = ParseAttributeValue();
                attributes[name] = value;
            }
            return attributes;
        }

        public string ParseAttributeName()
        {
            SkipWhitespace();
            var sb = new StringBuilder();
            while (!IsChar('='))
            {
                sb.Append(PeekChar());
                AdvancePosition();
            }
            ConsumeChar('=');
            return sb.ToString();
        }

        public string ParseAttributeValue()
        {
            SkipWhitespace();
            ConsumeChar('"');
            var sb = new StringBuilder();
            while (!IsChar('"'))
            {
                sb.Append(PeekChar());
                AdvancePosition();
            }
            ConsumeChar('"');
            return sb.ToString();
        }

        private char PeekChar()
        {
            CheckPosition();
            return xml[position];
        }

        private string PeekChars(int amount)
        {
            CheckPosition(amount - 1);
            return xml[position..(position + amount)];
        }

        private bool IsChar(char c)
        {
            return PeekChar() == c;
        }

        private bool IsString(string s)
        {
            return PeekChars(s.Length) == s;
        }

        private void ConsumeChar(char expected)
        {
            if (!IsChar(expected))
            {
                throw new System.Exception($"Expected '{expected}' at {position}, found  {xml[position]}");
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

        private void CheckPosition(int offset = 0)
        {
            if (xml.Length <= position + offset)
            {
                throw new System.Exception($"Unexpected end of xml");
            }
        }

        private void SkipWhitespace()
        {
            CheckPosition();
            while (char.IsWhiteSpace(xml[position]))
            {
                AdvancePosition();
            }
        }
    }
}
