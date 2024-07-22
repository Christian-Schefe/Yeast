using System.Collections.Generic;
using Yeast.Memento;
using Yeast.Utils;

namespace Yeast.Xml
{
    public class ToXmlMementoVisitor : IMementoVisitor<XmlDocument>
    {
        private List<XmlValue> children = new();
        private Dictionary<string, string> parentAttributes = new();

        public XmlDocument GetResult()
        {
            var root = new XmlElement("root", parentAttributes, children);
            return new XmlDocument(root, "1.0", "UTF-8");
        }

        public void Visit(NullMemento memento)
        {
            parentAttributes = new() { { "null", "true" } };
            children = new();
        }

        public void Visit(StringMemento memento)
        {
            parentAttributes = new();
            children = new() { new XmlString(memento.value) };
        }

        public void Visit(IntegerMemento memento)
        {
            parentAttributes = new();
            children = new() { new XmlString(StringUtils.LongToString(memento.value)) };
        }

        public void Visit(DecimalMemento memento)
        {
            parentAttributes = new();
            children = new() { new XmlString(StringUtils.DoubleToString(memento.value)) };
        }

        public void Visit(BoolMemento memento)
        {
            parentAttributes = new();
            children = new() { new XmlString(memento.value ? "true" : "false") };
        }

        public void Visit(ArrayMemento memento)
        {
            var list = new List<XmlValue>();
            foreach (var item in memento.value)
            {
                item.Accept(this);
                list.Add(new XmlElement("element", parentAttributes, children));
            }
            parentAttributes = new();
            children = list;
        }

        public void Visit(DictMemento memento)
        {
            var list = new List<XmlValue>();
            var attributes = new Dictionary<string, string>();
            foreach (var (key, value) in memento.value)
            {
                if (key == "$type") attributes.Add("derivedType", ((StringMemento)value).value);
                else
                {
                    value.Accept(this);
                    list.Add(new XmlElement(key, parentAttributes, children));
                }
            }
            parentAttributes = attributes;
            children = list;
        }
    }


    public class ToIntegerMementoXmlVisitor : IXmlVisitor<IMemento>
    {
        public IntegerMemento result;

        public IMemento GetResult()
        {
            return result;
        }

        public void Visit(XmlString xml)
        {
            result = new IntegerMemento(StringUtils.StringToLong(xml.value));
        }

        public void Visit(XmlElement xml)
        {
            var name = xml.name.ToLowerInvariant();
            if (xml.children.Count == 0)
            {
                if (name == "zero")
                {
                    result = new IntegerMemento(0);
                }
                else if (name == "one")
                {
                    result = new IntegerMemento(1);
                }
                else
                {
                    throw new System.InvalidOperationException("Cannot convert XmlElement to IntegerMemento");
                }
            }
            else if (xml.children.Count == 1)
            {
                if ((name == "integer" || name == "int") && xml.children[0] is XmlString str)
                {
                    Visit(str);
                }
                else
                {
                    throw new System.InvalidOperationException("Cannot convert XmlElement to IntegerMemento");
                }
            }
            else
            {
                throw new System.InvalidOperationException("Cannot convert XmlElement to IntegerMemento");
            }
        }
    }

    public class ToDecimalMementoXmlVisitor : IXmlVisitor<IMemento>
    {
        public DecimalMemento result;

        public IMemento GetResult()
        {
            return result;
        }

        public void Visit(XmlString xml)
        {
            result = new DecimalMemento(StringUtils.StringToDouble(xml.value));
        }

        public void Visit(XmlElement xml)
        {
            var name = xml.name.ToLowerInvariant();
            if (xml.children.Count == 0)
            {
                if (name == "zero")
                {
                    result = new DecimalMemento(0);
                }
                else if (name == "one")
                {
                    result = new DecimalMemento(1);
                }
                else
                {
                    throw new System.InvalidOperationException("Cannot convert XmlElement to DecimalMemento");
                }
            }
            else if (xml.children.Count == 1)
            {
                if ((name == "float" || name == "double") && xml.children[0] is XmlString str)
                {
                    Visit(str);
                }
                else
                {
                    throw new System.InvalidOperationException("Cannot convert XmlElement to DecimalMemento");
                }
            }
            else
            {
                throw new System.InvalidOperationException("Cannot convert XmlElement to DecimalMemento");
            }
        }
    }

    public class ToBoolMementoXmlVisitor : IXmlVisitor<IMemento>
    {
        public BoolMemento result;

        public IMemento GetResult()
        {
            return result;
        }

        public void Visit(XmlString xml)
        {
            var str = xml.value.ToLowerInvariant();
            result = str switch
            {
                "true" => new BoolMemento(true),
                "false" => new BoolMemento(false),
                "1" => new BoolMemento(true),
                "0" => new BoolMemento(false),
                _ => throw new System.InvalidOperationException("Cannot convert XmlString to BoolMemento")
            };
        }

        public void Visit(XmlElement xml)
        {
            var name = xml.name.ToLowerInvariant();
            if (xml.children.Count == 0)
            {
                if (name == "true")
                {
                    result = new BoolMemento(true);
                }
                else if (name == "false")
                {
                    result = new BoolMemento(false);
                }
                else
                {
                    throw new System.InvalidOperationException("Cannot convert XmlElement to BoolMemento");
                }
            }
            else if (xml.children.Count == 1)
            {
                if ((name == "bool" || name == "boolean") && xml.children[0] is XmlString str)
                {
                    Visit(str);
                }
                else
                {
                    throw new System.InvalidOperationException("Cannot convert XmlElement to BoolMemento");
                }
            }
            else
            {
                throw new System.InvalidOperationException("Cannot convert XmlElement to BoolMemento");
            }
        }
    }
}
