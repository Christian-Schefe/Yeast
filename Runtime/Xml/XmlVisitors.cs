using System;
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
            var name = xml.name.ToLowerInvariant();
            if (xml.children.Count == 0)
            {
                if (name == "str" || name == "string")
                {
                    result = new StringMemento("");
                }
                else
                {
                    throw new InvalidOperationException("Cannot convert XmlElement to IntegerMemento");
                }
            }
            else if (xml.children.Count == 1)
            {
                if ((name == "string" || name == "str") && xml.children[0] is XmlString str)
                {
                    Visit(str);
                }
                else
                {
                    throw new InvalidOperationException("Cannot convert XmlElement to IntegerMemento");
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot convert XmlElement to IntegerMemento");
            }
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
                    throw new InvalidOperationException("Cannot convert XmlElement to IntegerMemento");
                }
            }
            else if (xml.children.Count == 1)
            {
                if ((name == "integer" || name == "int") && xml.children[0] is XmlString str)
                {
                    Visit(str);
                }
                else if ((name == "character" || name == "char") && xml.children[0] is XmlString ch && ch.value.Length == 1)
                {
                    result = new IntegerMemento(ch.value[0]);
                }
                else
                {
                    throw new InvalidOperationException("Cannot convert XmlElement to IntegerMemento");
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot convert XmlElement to IntegerMemento");
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
                    throw new InvalidOperationException("Cannot convert XmlElement to DecimalMemento");
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
                    throw new InvalidOperationException("Cannot convert XmlElement to DecimalMemento");
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot convert XmlElement to DecimalMemento");
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
                _ => throw new InvalidOperationException("Cannot convert XmlString to BoolMemento")
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
                    throw new InvalidOperationException("Cannot convert XmlElement to BoolMemento");
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
                    throw new InvalidOperationException("Cannot convert XmlElement to BoolMemento");
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot convert XmlElement to BoolMemento");
            }
        }
    }

    public class XmlTypeWrapperVisitor : TypeWrapperVisitor<XmlElement, IMemento>
    {
        public override void Visit(StringTypeWrapper stringTypeWrapper)
        {
            if (value.children.Count == 0)
            {
                if (value.attributes.TryGetValue("null", out var isNull) && isNull == "true")
                {
                    result = new NullMemento();
                }
                else
                {
                    result = new StringMemento("");
                }
            }
            else if (value.children.Count != 1)
            {
                throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to StringMemento");
            }
            else
            {
                var visitor = new ToStringMementoXmlVisitor();
                value.children[0].Accept(visitor);
                result = visitor.GetResult();
            }
        }

        public override void Visit(BoolTypeWrapper boolTypeWrapper)
        {
            if (value.children.Count == 0 && boolTypeWrapper.IsNullable && value.attributes.TryGetValue("null", out var isNull) && isNull == "true")
            {
                result = new NullMemento();
            }
            else if (value.children.Count != 1)
            {
                throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to BoolMemento");
            }
            else
            {
                var visitor = new ToBoolMementoXmlVisitor();
                value.children[0].Accept(visitor);
                result = visitor.GetResult();
            }
        }

        public override void Visit(IntegerTypeWrapper integerTypeWrapper)
        {
            if (value.children.Count == 0 && integerTypeWrapper.IsNullable && value.attributes.TryGetValue("null", out var isNull) && isNull == "true")
            {
                result = new NullMemento();
            }
            else if (value.children.Count != 1)
            {
                throw new InvalidOperationException($"Cannot convert {value} {value.GetType().Name} to IntegerMemento");
            }
            else
            {
                var visitor = new ToIntegerMementoXmlVisitor();
                value.children[0].Accept(visitor);
                result = visitor.GetResult();
            }
        }

        public override void Visit(RationalTypeWrapper rationalTypeWrapper)
        {
            if (value.children.Count == 0 && rationalTypeWrapper.IsNullable && value.attributes.TryGetValue("null", out var isNull) && isNull == "true")
            {
                result = new NullMemento();
            }
            else if (value.children.Count != 1)
            {
                throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to DecimalMemento");
            }
            else
            {
                var visitor = new ToDecimalMementoXmlVisitor();
                value.children[0].Accept(visitor);
                result = visitor.GetResult();
            }
        }

        public override void Visit(CollectionTypeWrapper collectionTypeWrapper)
        {
            void ConvertArray(XmlElement val, int rank)
            {
                List<IMemento> list = new();

                foreach (var item in val.children)
                {
                    if (item is not XmlElement xmlArray)
                    {
                        throw new InvalidOperationException($"Cannot convert {val.GetType().Name} to ArrayMemento");
                    }
                    if (rank == 1) Convert(xmlArray, collectionTypeWrapper.ElementType);
                    else ConvertArray(xmlArray, rank - 1);
                    list.Add(result);
                }

                result = new ArrayMemento(list);
            }

            if (value.attributes.TryGetValue("null", out var isNull) && isNull == "true")
            {
                result = new NullMemento();
            }
            else
            {
                ConvertArray(value, collectionTypeWrapper.Rank);
            }
        }

        public override void Visit(StructTypeWrapper structTypeWrapper)
        {
            if (structTypeWrapper.IsNullable && value.attributes.TryGetValue("null", out var isNull) && isNull == "true")
            {
                result = new NullMemento();
                return;
            }

            if (value is not XmlElement xmlObject)
            {
                throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to DictMemento");
            }

            StructTypeWrapper typeWrapper = structTypeWrapper;

            var obj = new Dictionary<string, IMemento>();

            if (xmlObject.attributes.TryGetValue("derivedType", out var typeIdentifier))
            {
                if (structTypeWrapper.DerivedTypes.ContainsKey(typeIdentifier))
                {
                    typeWrapper = structTypeWrapper.DerivedTypes[typeIdentifier];
                }
                obj.Add("$type", new StringMemento(typeIdentifier));
            }

            foreach (var child in xmlObject.children)
            {
                if (child is not XmlElement childObject)
                {
                    throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to DictMemento");
                }
                var fieldName = childObject.name;
                var res = Convert(childObject, typeWrapper.Fields[fieldName].type);
                obj.Add(fieldName, res);
            }

            result = new DictMemento(obj);
        }

        public override void Visit(RuntimeTypeTypeWrapper typeWrapper)
        {
            if (value.children.Count != 1)
            {
                throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to StringMemento");
            }
            if (value.children[0] is not XmlString jsonStr)
            {
                throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to StringMemento");
            }
            result = new StringMemento(jsonStr.value);
        }
    }
}
