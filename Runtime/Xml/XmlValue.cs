using System.Collections.Generic;
using System.Text;
using Yeast.Utils;

namespace Yeast.Xml
{
    public abstract class XmlValue
    {
        public abstract override string ToString();

        public static XmlValue FromString(string xml)
        {
            var parser = new XmlParser(xml);
            return parser.ParseValue();
        }

        public abstract void Accept(IXmlVisitor visitor);
    }

    public class XmlElement : XmlValue
    {
        public string name;
        public List<XmlValue> children;
        public Dictionary<string, string> attributes;

        public XmlElement(string name, Dictionary<string, string> attributes, List<XmlValue> children)
        {
            this.name = name;
            this.children = children;
            this.attributes = attributes;
        }

        public override void Accept(IXmlVisitor visitor) => visitor.Visit(this);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"<{name}");

            foreach (var (key, value) in attributes)
                sb.Append($" {key}=\"{value}\"");

            if (children.Count == 0)
            {
                sb.Append("/>");
                return sb.ToString();
            }

            sb.Append(">");
            foreach (var child in children)
                sb.Append(child.ToString());
            sb.Append($"</{name}>");

            return sb.ToString();
        }
    }

    public class XmlString : XmlValue
    {
        public string value;

        public XmlString(string value)
        {
            this.value = value;
        }

        public override void Accept(IXmlVisitor visitor) => visitor.Visit(this);

        public override string ToString() => value.Length == 0 ? "<EmptyString/>" : StringUtils.EscapeJsonString(StringUtils.EscapeXMLString(value));
    }

    public interface IXmlVisitor
    {
        void Visit(XmlElement element);
        void Visit(XmlString element);
    }

    public interface IXmlVisitor<T> : IXmlVisitor
    {
        T GetResult();
    }
}
