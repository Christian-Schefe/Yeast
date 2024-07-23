using System;
using System.Collections.Generic;
using Yeast.Utils;
using Yeast.Xml;

namespace Yeast.Memento
{
    public class XmlToMementoTranslator
    {
        public IMemento Convert(XmlElement xmlElement, Type type)
        {
            if (xmlElement.attributes.TryGetValue("null", out var nullValue) && nullValue == "true")
            {
                if (!TypeUtils.IsNullable(type))
                {
                    throw new InvalidOperationException("Cannot convert null to non-nullable type");
                }
                return new NullMemento();
            }
            type = ICustomTransformer.GetDeserializationType(type);
            type = Nullable.GetUnderlyingType(type) ?? type;
            return ConvertInternal(xmlElement, type);
        }

        public IMemento ConvertInternal(XmlElement xmlElement, Type type)
        {
            if (type == typeof(string))
            {
                if (xmlElement.children.Count == 0) return new StringMemento("");
                else if (xmlElement.children.Count == 1 && xmlElement.children[0] is XmlString xmlString)
                {
                    return new StringMemento(xmlString.value);
                }
                else throw new InvalidOperationException("Cannot convert XmlElement to StringMemento");
            }
            else if (type == typeof(bool))
            {
                if (xmlElement.children.Count != 1) throw new InvalidOperationException("Cannot convert XmlElement to BoolMemento");
                var visitor = new ToBoolMementoXmlVisitor();
                xmlElement.children[0].Accept(visitor);
                return visitor.GetResult();
            }
            else if (TypeUtils.IsIntegerNumber(type) || type.IsEnum || type == typeof(char))
            {
                if (xmlElement.children.Count != 1) throw new InvalidOperationException("Cannot convert XmlElement to IntegerMemento");
                var visitor = new ToIntegerMementoXmlVisitor();
                xmlElement.children[0].Accept(visitor);
                return visitor.GetResult();
            }
            else if (TypeUtils.IsRationalNumber(type))
            {
                if (xmlElement.children.Count != 1) throw new InvalidOperationException("Cannot convert XmlElement to DecimalMemento");
                var visitor = new ToDecimalMementoXmlVisitor();
                xmlElement.children[0].Accept(visitor);
                return visitor.GetResult();
            }
            else if (type.IsArray)
            {
                var elementType = type.GetElementType();
                IMemento ConvertArray(XmlElement arr, Type type, int rank)
                {
                    List<IMemento> list = new();
                    foreach (var item in arr.children)
                    {
                        var el = (XmlElement)item;
                        if (rank != 1)
                        {
                            if (el.children.Count == 0) list.Add(new ArrayMemento(new List<IMemento>()));
                            else list.Add(ConvertArray(el, type, rank - 1));
                        }
                        else list.Add(Convert(el, elementType));
                    }
                    return new ArrayMemento(list);
                }
                return ConvertArray(xmlElement, type, type.GetArrayRank());
            }
            else if (TypeUtils.IsCollection(type, out var elementType))
            {
                var arr = new List<IMemento>();
                foreach (var item in xmlElement.children)
                {
                    var el = (XmlElement)item;
                    arr.Add(Convert(el, elementType));
                }
                return new ArrayMemento(arr);
            }
            else if (type.IsClass || TypeUtils.IsStruct(type))
            {
                var obj = new Dictionary<string, IMemento>();

                if (xmlElement.attributes.TryGetValue("derivedType", out var typeIdentifier))
                {
                    if (TypeUtils.TryGetDerivedTypeById(type, typeIdentifier, out var derivedType))
                    {
                        type = derivedType;
                    }
                    obj.Add("$type", new StringMemento(typeIdentifier));
                }

                var typeDict = new Dictionary<string, Type>();
                foreach (var field in TypeUtils.GetFields(type))
                {
                    typeDict[field.Name] = field.FieldType;
                }

                foreach (var item in xmlElement.children)
                {
                    var el = (XmlElement)item;
                    obj.Add(el.name, Convert(el, typeDict[el.name]));
                }

                return new DictMemento(obj);
            }
            else
            {
                throw new NotSupportedException($"Type {type} is not supported");
            }
        }
    }
}
