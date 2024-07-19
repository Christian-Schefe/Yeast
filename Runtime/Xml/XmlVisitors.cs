using System.Collections.Generic;
using Yeast.Memento;
using Yeast.Utils;

namespace Yeast.Xml
{
    public class ToXmlMementoVisitor : IMementoVisitor<XmlValue>
    {
        public XmlValue result;

        public XmlValue GetResult()
        {
            return result;
        }

        public void Visit(NullMemento memento)
        {
            result = new XmlElement("Null", new(), new());
        }

        public void Visit(StringMemento memento)
        {
            result = new XmlString(memento.value);
        }

        public void Visit(IntegerMemento memento)
        {
            result = new XmlString(StringUtils.LongToString(memento.value));
        }

        public void Visit(DecimalMemento memento)
        {
            result = new XmlString(StringUtils.DoubleToString(memento.value));
        }

        public void Visit(BoolMemento memento)
        {
            result = new XmlString(memento.value ? "true" : "false");
        }

        public void Visit(ArrayMemento memento)
        {
            var list = new List<XmlValue>();
            foreach (var item in memento.value)
            {
                item.Accept(this);
                list.Add(new XmlElement("Item", new(), new() { result }));
            }
            result = new XmlElement("List", new(), list);
        }

        public void Visit(DictMemento memento)
        {
            var list = new List<XmlValue>();
            var attributes = new Dictionary<string, string>();
            foreach (var (key, value) in memento.value)
            {
                if (key == "$type") attributes.Add("$type", ((StringMemento)value).value);
                else
                {
                    value.Accept(this);
                    list.Add(new XmlElement(key, new(), new() { result }));
                }
            }
            result = new XmlElement("Object", attributes, list);
        }
    }

    public class ToStringMementoXmlVisitor : IXmlVisitor<IMemento>
    {
        public StringMemento result;

        public IMemento GetResult()
        {
            return result;
        }

        public void Visit(XmlString xml)
        {
            result = new StringMemento(xml.value);
        }

        public void Visit(XmlElement xml)
        {
            throw new System.InvalidOperationException("Cannot convert XmlElement to StringMemento");
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
            throw new System.InvalidOperationException("Cannot convert XmlElement to IntegerMemento");
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
            throw new System.InvalidOperationException("Cannot convert XmlElement to DecimalMemento");
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
            result = xml.value switch
            {
                "true" => new BoolMemento(true),
                "false" => new BoolMemento(false),
                _ => throw new System.InvalidOperationException("Cannot convert XmlString to BoolMemento")
            };
        }

        public void Visit(XmlElement xml)
        {
            throw new System.InvalidOperationException("Cannot convert XmlElement to BoolMemento");
        }
    }
}
