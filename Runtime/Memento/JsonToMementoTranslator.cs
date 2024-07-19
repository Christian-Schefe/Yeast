using System;
using System.Collections;
using System.Collections.Generic;
using Yeast.Json;
using Yeast.Utils;
using static UnityEngine.Networking.UnityWebRequest;

namespace Yeast.Memento
{
    public class JsonToMementoTranslator
    {
        public IMemento Convert(JsonValue jsonValue, Type type)
        {
            if (jsonValue is JsonNull)
            {
                return new NullMemento();
            }

            Type underlyingType = Nullable.GetUnderlyingType(type);
            bool isNullableValueType = underlyingType != null;
            if (isNullableValueType) type = underlyingType;

            if (type == typeof(string))
            {
                var visitor = new ToStringMementoJsonVisitor();
                jsonValue.Accept(visitor);
                return visitor.GetResult();
            }
            else if (type == typeof(bool))
            {
                var visitor = new ToBoolMementoJsonVisitor();
                jsonValue.Accept(visitor);
                return visitor.GetResult();
            }
            else if (TypeUtils.IsIntegerNumber(type) || type.IsEnum)
            {
                var visitor = new ToIntegerMementoJsonVisitor();
                jsonValue.Accept(visitor);
                return visitor.GetResult();
            }
            else if (TypeUtils.IsRationalNumber(type))
            {
                var visitor = new ToDecimalMementoJsonVisitor();
                jsonValue.Accept(visitor);
                return visitor.GetResult();
            }
            else if (type.IsArray)
            {
                if (jsonValue is not JsonArray jsonArray)
                {
                    throw new InvalidOperationException($"Cannot convert {jsonValue.GetType().Name} to DictMemento");
                }
                var elementType = type.GetElementType();
                IMemento ConvertArray(JsonArray arr, Type type, int rank)
                {
                    List<IMemento> list = new();
                    foreach (var item in arr.value)
                    {
                        if (rank == 1) list.Add(Convert(item, elementType));
                        else list.Add(ConvertArray((JsonArray)item, type, rank - 1));
                    }
                    return new ArrayMemento(list);
                }
                return ConvertArray(jsonArray, type, type.GetArrayRank());
            }
            else if (TypeUtils.IsCollection(type, out var elementType))
            {
                if (jsonValue is not JsonArray jsonArray)
                {
                    throw new InvalidOperationException($"Cannot convert {jsonValue.GetType().Name} to DictMemento");
                }
                var arr = new List<IMemento>();
                foreach (var item in jsonArray.value)
                {
                    arr.Add(Convert(item, elementType));
                }
                return new ArrayMemento(arr);
            }
            else if (type.IsClass || TypeUtils.IsStruct(type))
            {
                if (jsonValue is not JsonObject jsonObject)
                {
                    throw new InvalidOperationException($"Cannot convert {jsonValue.GetType().Name} to DictMemento");
                }
                if (jsonObject.value.TryGetValue("$type", out var typeValue))
                {
                    var typeIdentifier = typeValue.AsString();
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
                }

                var typeDict = new Dictionary<string, Type>() { { "$type", typeof(string) } };
                foreach (var field in TypeUtils.GetFields(type))
                {
                    typeDict[field.Name] = field.FieldType;
                }

                var obj = new Dictionary<string, IMemento>();
                foreach (var pair in jsonObject.value)
                {
                    obj.Add(pair.Key, Convert(pair.Value, typeDict[pair.Key]));
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
