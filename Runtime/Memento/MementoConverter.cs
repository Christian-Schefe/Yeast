using System;
using System.Collections;
using System.Collections.Generic;
using Yeast.Utils;

namespace Yeast.Memento
{
    public struct ToMementoSettings
    {
        public int maxDepth;
    }

    public struct FromMementoSettings
    {
        public UseDefaultSetting useDefaultSetting;
        public bool ignoreExtraFields;

        public enum UseDefaultSetting
        {
            Never, ForMissingFields, ForMissingOrMismatchedFields
        }
    }

    public class MementoConversionException : Exception
    {
        public MementoConversionException(string message) : base(message) { }
    }

    public class InternalException : MementoConversionException
    {
        public InternalException(string message) : base(message) { }
    }

    public class TypeMismatchException : MementoConversionException
    {
        public TypeMismatchException(string message) : base(message) { }
    }

    public class CircularReferenceException : MementoConversionException
    {
        public CircularReferenceException(string message) : base(message) { }
    }

    public class MementoConverter : BaseObjectConverter<IMemento, ToMementoSettings, FromMementoSettings>
    {
        protected override IMemento Serialize(object value)
        {
            return Serialize(value, 0);
        }

        private IMemento Serialize(object value, int depth)
        {
            if (depth > serializationSettings.maxDepth)
            {
                throw new CircularReferenceException("Recursion limit reached. Your object may contain circular references.");
            }

            if (value == null)
            {
                return new NullMemento();
            }
            Type type = value.GetType();

            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                throw new TypeMismatchException("Cannot stringify instances of UnityEngine.Object.");
            }

            if (value is bool boolValue)
            {
                return new BoolMemento(boolValue);
            }
            else if (value is string stringValue)
            {
                return new StringMemento(stringValue);
            }
            else if (TypeUtils.IsIntegerNumber(type) || type == typeof(char) || type.IsEnum)
            {
                long val = Convert.ToInt64(value);
                return new IntegerMemento(val);
            }
            else if (TypeUtils.IsRationalNumber(type))
            {
                double val = Convert.ToDouble(value);
                return new DecimalMemento(val);
            }
            else if (TypeUtils.IsCollection(type, out _))
            {
                IEnumerable collection = (IEnumerable)value;
                var intermediateArray = new List<IMemento>();
                foreach (var element in collection)
                {
                    var el = Serialize(element, depth + 1);
                    intermediateArray.Add(el);
                }
                return new ArrayMemento(intermediateArray);
            }
            else if (type.IsArray) // must be multi-dimensional, as single-dimensional arrays implement ICollection<T>
            {
                Array array = (Array)value;
                return ArrayUtils.ArrayToMemento(Serialize, array);
            }
            else
            {
                var intermediateMap = new Dictionary<string, IMemento>();
                var fields = TypeUtils.GetFields(type);
                foreach (var field in fields)
                {
                    var val = field.GetValue(value);
                    var el = Serialize(val, depth + 1);
                    intermediateMap.Add(field.Name, el);
                }

                if (TypeUtils.HasSingleAttribute(type, out IsDerivedClassAttribute attr))
                {
                    if (!TypeUtils.HasAttribute<HasDerivedClassAttribute>(attr.BaseType, out var attrs))
                    {
                        throw new TypeMismatchException("Cannot serialize derived class without HasDerivedClassAttribute.");
                    }
                    var derivedClassAttr = attrs.Find(a => a.DerivedType == type)
                        ?? throw new TypeMismatchException("Cannot serialize derived class without HasDerivedClassAttribute.");
                    intermediateMap.Add("$type", new StringMemento(derivedClassAttr.Identifier));
                }

                return new DictMemento(intermediateMap);
            }
        }

        protected override object Deserialize(Type type, IMemento value)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                throw new TypeMismatchException("Cannot deserialize to new instances of type '" + type.Name + ".'");
            }

            bool isInstantiable = TypeUtils.IsInstantiateable(type);
            bool hasDerivedClassAttribute = TypeUtils.HasAttribute<HasDerivedClassAttribute>(type, out var attrs);
            if (!isInstantiable && !hasDerivedClassAttribute)
            {
                throw new TypeMismatchException("Cannot deserialize to new instances of type '" + type.Name + ".'");
            }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null) type = nullableType;
            bool isNullable = nullableType != null;

            if (value is NullMemento)
            {
                if (type == typeof(object) || type.IsClass || isNullable)
                {
                    return null;
                }
                throw new TypeMismatchException("Failed to convert null to " + type.Name + ": type not nullable");
            }
            else if (value is StringMemento stringValue)
            {
                if (type == typeof(string) || type == typeof(object))
                {
                    return stringValue.value;
                }
                throw new TypeMismatchException("Failed to convert string to " + type.Name);
            }
            else if (value is IntegerMemento integerValue)
            {
                if (type == typeof(object))
                {
                    return integerValue.value;
                }
                else if (TypeUtils.IsIntegerNumber(type) || TypeUtils.IsRationalNumber(type))
                {
                    return Convert.ChangeType(integerValue.value, type);
                }
                else if (type.IsEnum)
                {
                    return Enum.ToObject(type, integerValue.value);
                }
                throw new TypeMismatchException("Failed to convert long to " + type.Name);
            }
            else if (value is DecimalMemento floatValue)
            {
                if (TypeUtils.IsRationalNumber(type))
                {
                    return Convert.ChangeType(floatValue.value, type);
                }
                else if (type == typeof(object))
                {
                    return floatValue.value;
                }
                throw new TypeMismatchException("Failed to convert double to " + type.Name);
            }
            else if (value is BoolMemento boolValue)
            {
                if (type == typeof(bool) || type == typeof(object))
                {
                    return boolValue.value;
                }
                throw new TypeMismatchException("Failed to convert bool to " + type.Name);
            }
            else if (value is ArrayMemento arrayValue)
            {
                if (type == typeof(object))
                {
                    var array = new object[arrayValue.value.Length];
                    for (int i = 0; i < arrayValue.value.Length; i++)
                    {
                        array[i] = Deserialize(typeof(object), arrayValue.value[i]);
                    }
                    return array;
                }
                else if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    object Transform(IMemento val) => Deserialize(elementType, val);

                    var arr = ArrayUtils.MementoToArray(elementType, type, Transform, arrayValue);
                    return arr;
                }
                else if (TypeUtils.IsCollection(type, out var elementType))
                {
                    if (!TypeUtils.TryCreateInstance(type, out object instance))
                    {
                        throw new TypeMismatchException("Failed to create instance of " + type.Name);
                    }
                    var wrapper = new CollectionWrapper(type, elementType, instance);
                    foreach (var element in arrayValue.value)
                    {
                        var elementObject = Deserialize(elementType, element);
                        wrapper.Add(elementObject);
                    }
                    return wrapper.GetCollection();
                }
                throw new TypeMismatchException("Failed to convert array to " + type.Name);
            }
            else if (value is DictMemento mapValue)
            {
                if (type == typeof(object))
                {
                    var instance = new Dictionary<string, object>();
                    foreach (var pair in mapValue.value)
                    {
                        object pairValue = Deserialize(typeof(object), pair.Value);
                        instance.Add(pair.Key, pairValue);
                    }
                    return instance;
                }
                else if (type.IsClass || TypeUtils.IsStruct(type))
                {
                    string typeID = mapValue.value.ContainsKey("$type") ? ((StringMemento)mapValue.value["$type"]).value : null;
                    if (!isInstantiable && typeID == null)
                    {
                        throw new TypeMismatchException("Failed to convert map to " + type.Name + ": type is not instantiateable and there is no type ID");
                    }

                    if (typeID != null)
                    {
                        var attr = attrs.Find(a => a.Identifier == typeID)
                            ?? throw new TypeMismatchException("Failed to convert map to " + type.Name + ": type ID does not match any derived class");
                        if (!type.IsAssignableFrom(attr.DerivedType))
                        {
                            throw new TypeMismatchException("Failed to convert map to " + type.Name + $": {attr.DerivedType.Name} is not a derived type");
                        }
                        mapValue.value.Remove("$type");
                        return Deserialize(attr.DerivedType, mapValue);
                    }

                    if (!TypeUtils.TryCreateInstance(type, out object instance))
                    {
                        throw new TypeMismatchException("Failed to create instance of " + type.Name);
                    }
                    var unusedKeys = new HashSet<string>(mapValue.value.Keys);
                    foreach (var field in TypeUtils.GetFields(type))
                    {
                        string fieldName = field.Name;
                        if (!mapValue.value.TryGetValue(fieldName, out IMemento fieldValue))
                        {
                            if (deserializationSettings.useDefaultSetting == FromMementoSettings.UseDefaultSetting.ForMissingFields
                                || deserializationSettings.useDefaultSetting == FromMementoSettings.UseDefaultSetting.ForMissingOrMismatchedFields)
                            {
                                field.SetValue(instance, TypeUtils.DefaultValue(type));
                                unusedKeys.Remove(fieldName);
                                continue;
                            }
                            throw new TypeMismatchException("Failed to convert map to " + type.Name + ": missing field " + fieldName);
                        }
                        try
                        {
                            object fieldValueObject = Deserialize(field.FieldType, fieldValue);
                            field.SetValue(instance, fieldValueObject);
                            unusedKeys.Remove(fieldName);
                            field.SetValue(instance, fieldValueObject);
                            unusedKeys.Remove(fieldName);
                        }
                        catch (TypeMismatchException e)
                        {
                            if (deserializationSettings.useDefaultSetting == FromMementoSettings.UseDefaultSetting.ForMissingOrMismatchedFields)
                            {
                                field.SetValue(instance, TypeUtils.DefaultValue(type));
                                unusedKeys.Remove(fieldName);
                                continue;
                            }
                            throw e;
                        }
                    }
                    if (!deserializationSettings.ignoreExtraFields && unusedKeys.Count > 0)
                    {
                        throw new TypeMismatchException("Failed to convert map to " + type.Name + ": extra fields");
                    }
                    return instance;
                }
                throw new TypeMismatchException("Failed to convert map to " + type.Name);
            }
            else
            {
                throw new InternalException("Unknown Memento Type" + type.Name);
            }
        }
    }
}
