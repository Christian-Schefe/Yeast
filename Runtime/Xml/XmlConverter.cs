using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Yeast.Json;
using Yeast.Memento;
using Yeast.Utils;

namespace Yeast.Xml
{
    public struct XmlSerializationSettings { }
    public struct XmlDeserializationSettings { }

    public class XmlConverter : BaseMementoConverter<string, XmlSerializationSettings, XmlDeserializationSettings>
    {
        protected override IMemento Deserialize(string text)
        {
            int position = 0;
            ReadTag(text, ref position, out var tagName, out var attributes);
            var result = Deserialize(text, ref position, attributes);
            ExpectString($"</{tagName}>", text, ref position);
            return result;
        }

        private IMemento Deserialize(string text, ref int position, Dictionary<string, string> attributes)
        {
            var type = attributes["type"];
            return type switch
            {
                "Null" => ReadNull(text, ref position),
                "Integer" => ReadInteger(text, ref position),
                "String" => ReadString(text, ref position),
                "Decimal" => ReadDouble(text, ref position),
                "Bool" => ReadBool(text, ref position),
                "Array" => ReadArray(text, int.Parse(attributes["length"]), ref position),
                "Dict" => ReadDict(text, int.Parse(attributes["length"]), ref position),
                _ => throw new JsonConversionException($"Unknown memento type: {type}", position)
            };
        }

        private IMemento ReadNull(string text, ref int position)
        {
            ExpectString("null", text, ref position);
            return new NullMemento();
        }

        private IMemento ReadInteger(string text, ref int position)
        {
            StringBuilder num = new();
            while (!IsChar('<', text, ref position))
            {
                num.Append(text[position]);
                AdvancePosition(text, ref position);
            }
            return new IntegerMemento(StringUtils.StringToLong(num.ToString()));
        }

        private IMemento ReadDouble(string text, ref int position)
        {
            StringBuilder num = new();
            while (!IsChar('<', text, ref position))
            {
                num.Append(text[position]);
                AdvancePosition(text, ref position);
            }
            return new DecimalMemento(StringUtils.StringToDouble(num.ToString()));
        }

        private IMemento ReadString(string text, ref int position)
        {
            StringBuilder str = new();
            while (!IsChar('<', text, ref position))
            {
                str.Append(text[position]);
                AdvancePosition(text, ref position);
            }
            return new StringMemento(StringUtils.HTMLUnescape(StringUtils.UnescapeString(str.ToString())));
        }

        private IMemento ReadBool(string text, ref int position)
        {
            StringBuilder str = new();
            while (!IsChar('<', text, ref position))
            {
                str.Append(text[position]);
                AdvancePosition(text, ref position);
            }
            UnityEngine.Debug.Log(str.ToString());
            return new BoolMemento(str.ToString().Equals("true"));
        }

        private IMemento ReadArray(string text, int length, ref int position)
        {
            List<IMemento> items = new();
            for (int i = 0; i < length; i++)
            {
                ReadTag(text, ref position, out var tagName, out var attributes);
                items.Add(Deserialize(text, ref position, attributes));
                ExpectString($"</{tagName}>", text, ref position);
            }
            return new ArrayMemento(items);
        }

        private IMemento ReadDict(string text, int length, ref int position)
        {
            Dictionary<string, IMemento> items = new();
            for (int i = 0; i < length; i++)
            {
                ReadTag(text, ref position, out var tagName, out var attributes);
                items[tagName] = Deserialize(text, ref position, attributes);
                ExpectString($"</{tagName}>", text, ref position);
            }
            return new DictMemento(items);
        }

        private void ReadTag(string text, ref int position, out string tagName, out Dictionary<string, string> attributes)
        {
            ExpectChar('<', text, ref position);
            StringBuilder tag = new();
            while (!IsChar('>', text, ref position))
            {
                tag.Append(text[position]);
                AdvancePosition(text, ref position);
            }
            AdvancePosition(text, ref position);
            var parts = tag.ToString().Split(' ');
            tagName = parts[0];
            attributes = new();
            for (int i = 1; i < parts.Length; i++)
            {
                var attr = parts[i].Split('=');
                attributes[attr[0]] = attr[1].Trim('"');
            }
        }

        protected override string Serialize(IMemento value)
        {
            StringBuilder result = new();
            AddTag(result, value, "data");
            Serialize(result, value);
            result.Append("</data>");
            return result.ToString();
        }

        private void AddTag(StringBuilder result, IMemento value, string tagName)
        {
            var mementoType = value.MementoType;
            result.Append('<');
            result.Append(tagName);
            int? length = mementoType switch
            {
                MementoType.Array => (value as ArrayMemento)?.value.Length,
                MementoType.Dict => (value as DictMemento)?.value.Count,
                _ => null
            };
            result.Append(" type=\"");
            result.Append(mementoType.ToString());
            result.Append('"');
            if (length is int len)
            {
                result.Append(" length=\"");
                result.Append(len);
                result.Append('"');
            }
            result.Append('>');
        }

        private void Serialize(StringBuilder result, IMemento value)
        {
            if (value is NullMemento)
            {
                result.Append("null");
            }
            else if (value is IntegerMemento intMemento)
            {
                result.Append(StringUtils.LongToString(intMemento.value));
            }
            else if (value is StringMemento strMemento)
            {
                result.Append(StringUtils.HTMLEscape(StringUtils.EscapeString(strMemento.value)));
            }
            else if (value is DecimalMemento decimalMemento)
            {
                result.Append(StringUtils.DoubleToString(decimalMemento.value));
            }
            else if (value is BoolMemento boolMemento)
            {
                result.Append(boolMemento.value ? "true" : "false");
            }
            else if (value is ArrayMemento arrMemento)
            {
                foreach (var item in arrMemento.value)
                {
                    AddTag(result, item, "item");
                    Serialize(result, item);
                    result.Append("</item>");
                }
            }
            else if (value is DictMemento dictMemento)
            {
                foreach (var item in dictMemento.value)
                {
                    AddTag(result, item.Value, item.Key);
                    Serialize(result, item.Value);
                    result.Append("</");
                    result.Append(item.Key);
                    result.Append('>');
                }
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
                throw new JsonConversionException($"Expected '{expected}', found {text[position]}", position);
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
