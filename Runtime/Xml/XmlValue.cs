using System.Collections.Generic;
using System.Text;
using Yeast.Utils;

namespace Yeast.Xml
{
    public abstract class XmlValue
    {
        public abstract override string ToString();

        public abstract void Accept(IXmlVisitor visitor);
    }

    public class XmlDocument
    {
        public XmlElement root;
        public string version;
        public string encoding;

        public XmlDocument(XmlElement root)
        {
            this.root = root;
            version = "1.0";
            encoding = "UTF-8";
        }

        public XmlDocument(XmlElement root, string version, string encoding)
        {
            this.root = root;
            this.version = version;
            this.encoding = encoding;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"<?xml version=\"{version}\" encoding=\"{encoding}\"?>");
            sb.Append(root.ToString());
            return sb.ToString();
        }

        public static XmlDocument FromString(string xml)
        {
            var parser = new XmlParser(xml);
            return parser.ParseDocument();
        }
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

            bool hasAddedSomething = false;
            foreach (var child in children)
            {
                var str = child.ToString();
                if (str.Length > 0)
                {
                    if (!hasAddedSomething)
                    {
                        sb.Append(">");
                        hasAddedSomething = true;
                    }
                    sb.Append(str);
                }
            }
            if (hasAddedSomething) sb.Append($"</{name}>");
            else sb.Append($"/>");

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

        public override string ToString() => StringUtils.EscapeJsonString(StringUtils.EscapeXMLString(value));
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
