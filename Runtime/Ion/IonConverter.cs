using System;
using System.Collections;
using System.Collections.Generic;
using Yeast.Utils;

namespace Yeast.Ion
{
    public struct ToIonSettings
    {
        public int maxDepth;
    }

    public struct FromIonSettings
    {
        public UseDefaultSetting useDefaultSetting;
        public bool ignoreExtraFields;

        public enum UseDefaultSetting
        {
            Never, ForMissingFields, ForMissingOrMismatchedFields
        }
    }

    public class IonConversionException : Exception
    {
        public IonConversionException(string message) : base(message) { }
    }

    public class InternalException : IonConversionException
    {
        public InternalException(string message) : base(message) { }
    }

    public class TypeMismatchException : IonConversionException
    {
        public TypeMismatchException(string message) : base(message) { }
    }

    public class CircularReferenceException : IonConversionException
    {
        public CircularReferenceException(string message) : base(message) { }
    }

    public class IonConverter : BaseObjectConverter<IIonValue, ToIonSettings, FromIonSettings>
    {
        protected override IIonValue Serialize(object value)
        {
            return Serialize(value, 0);
        }

        private IIonValue Serialize(object value, int depth)
        {
            if (depth > serializationSettings.maxDepth)
            {
                throw new CircularReferenceException("Recursion limit reached. Your object may contain circular references.");
            }

            if (value == null)
            {
                return new NullValue();
            }
            Type type = value.GetType();

            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                throw new TypeMismatchException("Cannot stringify instances of UnityEngine.Object.");
            }

            if (value is bool boolValue)
            {
                return new BooleanValue(boolValue);
            }
            else if (value is string stringValue)
            {
                return new StringValue(stringValue);
            }
            else if (TypeUtils.IsIntegerNumber(type) || type == typeof(char) || type.IsEnum)
            {
                long val = Convert.ToInt64(value);
                return new IntegerValue(val);
            }
            else if (TypeUtils.IsRationalNumber(type))
            {
                double val = Convert.ToDouble(value);
                return new FloatValue(val);
            }
            else if (TypeUtils.IsCollection(type, out _))
            {
                IEnumerable collection = (IEnumerable)value;
                var intermediateArray = new List<IIonValue>();
                foreach (var element in collection)
                {
                    var el = Serialize(element, depth + 1);
                    intermediateArray.Add(el);
                }
                return new ArrayValue(intermediateArray);
            }
            else if (type.IsArray) // must be multi-dimensional, as single-dimensional arrays implement ICollection<T>
            {
                Array array = (Array)value;
                return ArrayUtils.ArrayToIonValue(Serialize, array);
            }
            else
            {
                var intermediateMap = new Dictionary<string, IIonValue>();
                var fields = TypeUtils.GetFields(type);
                foreach (var field in fields)
                {
                    var val = field.GetValue(value);
                    var el = Serialize(val, depth + 1);
                    intermediateMap.Add(field.Name, el);
                }
                return new MapValue(intermediateMap);
            }
        }

        protected override object Deserialize(Type type, IIonValue value)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            else if (!TypeUtils.IsInstantiateable(type) || type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                throw new TypeMismatchException("Cannot deserialize to new instances of type '" + type.Name + ".'");
            }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null) type = nullableType;
            bool isNullable = nullableType != null;

            if (value is NullValue)
            {
                if (type == typeof(object) || type.IsClass || isNullable)
                {
                    return null;
                }
                throw new TypeMismatchException("Failed to convert null to " + type.Name + ": type not nullable");
            }
            else if (value is StringValue stringValue)
            {
                if (type == typeof(string) || type == typeof(object))
                {
                    return stringValue.value;
                }
                throw new TypeMismatchException("Failed to convert string to " + type.Name);
            }
            else if (value is IntegerValue integerValue)
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
            else if (value is FloatValue floatValue)
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
            else if (value is BooleanValue boolValue)
            {
                if (type == typeof(bool) || type == typeof(object))
                {
                    return boolValue.value;
                }
                throw new TypeMismatchException("Failed to convert bool to " + type.Name);
            }
            else if (value is ArrayValue arrayValue)
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
                    object Transform(IIonValue val) => Deserialize(elementType, val);

                    var arr = ArrayUtils.IonValueToArray(elementType, type, Transform, arrayValue);
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
            else if (value is MapValue mapValue)
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
                    if (!TypeUtils.TryCreateInstance(type, out object instance))
                    {
                        throw new TypeMismatchException("Failed to create instance of " + type.Name);
                    }
                    var unusedKeys = new HashSet<string>(mapValue.value.Keys);
                    foreach (var field in TypeUtils.GetFields(type))
                    {
                        string fieldName = field.Name;
                        if (!mapValue.value.TryGetValue(fieldName, out IIonValue fieldValue))
                        {
                            if (deserializationSettings.useDefaultSetting == FromIonSettings.UseDefaultSetting.ForMissingFields
                                || deserializationSettings.useDefaultSetting == FromIonSettings.UseDefaultSetting.ForMissingOrMismatchedFields)
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
                            if (deserializationSettings.useDefaultSetting == FromIonSettings.UseDefaultSetting.ForMissingOrMismatchedFields)
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
                throw new InternalException("Unknown IonValue Type" + type.Name);
            }
        }
    }
}
