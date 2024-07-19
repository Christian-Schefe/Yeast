using System;
using System.Collections.Generic;
using Yeast.Utils;
using Yeast.Xml;

namespace Yeast.Memento
{
    public class XmlToMementoTranslator
    {
        public IMemento Convert(XmlValue xmlValue, Type type)
        {
            if (xmlValue is XmlElement element && element.children.Count == 0)
            {
                if (element.name == "Null")
                {
                    return new NullMemento();
                }
                else if (element.name == "EmptyString")
                {
                    return new StringMemento("");
                }
            }

            Type underlyingType = Nullable.GetUnderlyingType(type);
            bool isNullableValueType = underlyingType != null;
            if (isNullableValueType) type = underlyingType;

            if (type == typeof(string))
            {
                var visitor = new ToStringMementoXmlVisitor();
                xmlValue.Accept(visitor);
                return visitor.GetResult();
            }
            else if (type == typeof(bool))
            {
                var visitor = new ToBoolMementoXmlVisitor();
                xmlValue.Accept(visitor);
                return visitor.GetResult();
            }
            else if (TypeUtils.IsIntegerNumber(type) || type.IsEnum)
            {
                var visitor = new ToIntegerMementoXmlVisitor();
                xmlValue.Accept(visitor);
                return visitor.GetResult();
            }
            else if (TypeUtils.IsRationalNumber(type))
            {
                var visitor = new ToDecimalMementoXmlVisitor();
                xmlValue.Accept(visitor);
                return visitor.GetResult();
            }
            else if (type.IsArray)
            {
                if (xmlValue is not XmlElement xmlArray)
                {
                    throw new InvalidOperationException($"Cannot convert {xmlValue.GetType().Name} to DictMemento");
                }
                var elementType = type.GetElementType();
                IMemento ConvertArray(XmlElement arr, Type type, int rank)
                {
                    List<IMemento> list = new();
                    foreach (var item in arr.children)
                    {
                        var el = ((XmlElement)item).children[0];
                        if (rank == 1) list.Add(Convert(el, elementType));
                        else list.Add(ConvertArray((XmlElement)el, type, rank - 1));
                    }
                    return new ArrayMemento(list);
                }
                return ConvertArray(xmlArray, type, type.GetArrayRank());
            }
            else if (TypeUtils.IsCollection(type, out var elementType))
            {
                if (xmlValue is not XmlElement xmlArray)
                {
                    throw new InvalidOperationException($"Cannot convert {xmlValue.GetType().Name} to DictMemento");
                }
                var arr = new List<IMemento>();
                foreach (var item in xmlArray.children)
                {
                    var el = ((XmlElement)item).children[0];
                    arr.Add(Convert(el, elementType));
                }
                return new ArrayMemento(arr);
            }
            else if (type.IsClass || TypeUtils.IsStruct(type))
            {
                if (xmlValue is not XmlElement xmlObject)
                {
                    throw new InvalidOperationException($"Cannot convert {xmlValue.GetType().Name} to DictMemento");
                }
                var obj = new Dictionary<string, IMemento>();

                if (xmlObject.attributes.TryGetValue("$type", out var typeIdentifier))
                {
                    if (TypeUtils.HasAttribute(type, out HasDerivedClassesAttribute attr))
                    {
                        foreach (var derivedType in attr.DerivedTypes)
                        {
                            if (TypeUtils.HasAttribute<IsDerivedClassAttribute>(derivedType, out var derivedAttr))
                            {
                                if (derivedAttr.Identifier == typeIdentifier)
                                {
                                    type = derivedType;
                                    break;
                                }
                            }
                        }
                    }
                    obj.Add("$type", new StringMemento(typeIdentifier));
                }

                var typeDict = new Dictionary<string, Type>();
                foreach (var field in TypeUtils.GetFields(type))
                {
                    typeDict[field.Name] = field.FieldType;
                }

                foreach (var item in xmlObject.children)
                {
                    var el = (XmlElement)item;
                    obj.Add(el.name, Convert(el.children[0], typeDict[el.name]));
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
