using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public class IonConverter : IIntoConverter<object, IIonValue, ToIonSettings, IonConversionException>, IFromConverter<(IIonValue, Type), object, FromIonSettings, IonConversionException>
    {
        private FromIonSettings fromIonSettings = new();
        private ToIonSettings toIonSettings = new();

        public bool TryInto(object value, out IIonValue result, ToIonSettings settings, out IonConversionException exception)
        {
            toIonSettings = settings;
            return ToIonValue(value, out result, out exception, 0);
        }

        public bool TryFrom((IIonValue, Type) value, out object result, FromIonSettings settings, out IonConversionException exception)
        {
            fromIonSettings = settings;
            return ToObject(value.Item2, value.Item1, out result, out exception);
        }

        private bool ToIonValue(object value, out IIonValue result, out IonConversionException exception, int depth)
        {
            exception = null;
            if (depth > toIonSettings.maxDepth)
            {
                result = null;
                exception = new CircularReferenceException("Recursion limit reached. Your object may contain circular references.");
                return false;
            }

            if (value == null)
            {
                result = new NullValue();
                return true;
            }
            Type type = value.GetType();

            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                throw new TypeMismatchException("Cannot stringify instances of UnityEngine.Object.");
            }

            if (value is bool boolValue)
            {
                result = new BooleanValue(boolValue);
            }
            else if (value is string stringValue)
            {
                result = new StringValue(stringValue);
            }
            else if (TypeUtils.IsIntegerNumber(type) || type == typeof(char) || type.IsEnum)
            {
                long val = Convert.ToInt64(value);
                result = new IntegerValue(val);
            }
            else if (TypeUtils.IsRationalNumber(type))
            {
                double val = Convert.ToDouble(value);
                result = new FloatValue(val);
            }
            else if (TypeUtils.IsCollection(type, out _))
            {
                IEnumerable collection = (IEnumerable)value;
                var intermediateArray = new List<IIonValue>();
                foreach (var element in collection)
                {
                    if (!ToIonValue(element, out var el, out var e, depth + 1))
                    {
                        result = null;
                        exception = e;
                        return false;
                    }
                    intermediateArray.Add(el);
                }
                result = new ArrayValue(intermediateArray);
            }
            else if (type.IsArray) // must be multi-dimensional, as single-dimensional arrays implement ICollection<T>
            {
                Array array = (Array)value;
                result = ConvertMultiDimArray(array, 0, depth);
            }
            else
            {
                var intermediateMap = new Dictionary<string, IIonValue>();
                var fields = TypeUtils.GetFields(type);
                foreach (var field in fields)
                {
                    var val = field.GetValue(value);
                    if (!ToIonValue(val, out var el, out var e, depth + 1))
                    {
                        result = null;
                        exception = e;
                        return false;
                    }
                    intermediateMap.Add(field.Name, el);
                }
                result = new MapValue(intermediateMap);
            }
            return true;
        }

        private bool ToObject(Type type, IIonValue value, out object result, out IonConversionException exception)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            else if (!TypeUtils.IsInstantiateable(type) || type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                throw new TypeMismatchException("Cannot deserialize to new instances of type '" + type.Name + ".'");
            }

            bool Accept(object val, out object result, out IonConversionException exception)
            {
                result = val;
                exception = null;
                return true;
            }

            bool Reject(IonConversionException e, out object result, out IonConversionException exception)
            {
                result = TypeUtils.DefaultValue(type);
                exception = e;
                return false;
            }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null) type = nullableType;
            bool isNullable = nullableType != null;

            if (value is NullValue)
            {
                if (type == typeof(object) || type.IsClass || isNullable)
                {
                    return Accept(null, out result, out exception);
                }
                return Reject(new TypeMismatchException("Failed to convert null to " + type.Name + ": type not nullable"), out result, out exception);
            }
            else if (value is StringValue stringValue)
            {
                if (type == typeof(string) || type == typeof(object))
                {
                    return Accept(stringValue.value, out result, out exception);
                }
                return Reject(new TypeMismatchException("Failed to convert string to " + type.Name), out result, out exception);
            }
            else if (value is IntegerValue integerValue)
            {
                if (type == typeof(object))
                {
                    return Accept(integerValue.value, out result, out exception);
                }
                else if (TypeUtils.IsIntegerNumber(type) || TypeUtils.IsRationalNumber(type))
                {
                    try
                    {
                        return Accept(Convert.ChangeType(integerValue.value, type), out result, out exception);
                    }
                    catch
                    {
                        return Reject(new InternalException("Failed to convert long to " + type.Name + ": cast failed"), out result, out exception);
                    }
                }
                else if (type.IsEnum)
                {
                    try
                    {
                        return Accept(Enum.ToObject(type, integerValue.value), out result, out exception);
                    }
                    catch
                    {
                        return Reject(new InternalException("Failed to convert long to " + type.Name + ": cast failed"), out result, out exception);
                    }
                }
                return Reject(new TypeMismatchException("Failed to convert long to " + type.Name), out result, out exception);
            }
            else if (value is FloatValue floatValue)
            {
                if (TypeUtils.IsRationalNumber(type))
                {
                    try
                    {
                        return Accept(Convert.ChangeType(floatValue.value, type), out result, out exception);
                    }
                    catch
                    {
                        return Reject(new InternalException("Failed to convert double to " + type.Name + ": cast failed"), out result, out exception);
                    }
                }
                else if (type == typeof(object))
                {
                    return Accept(floatValue.value, out result, out exception);
                }
                return Reject(new TypeMismatchException("Failed to convert double to " + type.Name), out result, out exception);
            }
            else if (value is BooleanValue boolValue)
            {
                if (type == typeof(bool) || type == typeof(object))
                {
                    return Accept(boolValue.value, out result, out exception);
                }
                return Reject(new TypeMismatchException("Failed to convert bool to " + type.Name), out result, out exception);
            }
            else if (value is ArrayValue arrayValue)
            {
                if (type == typeof(object) || type.IsArray)
                {
                    var rank = type.GetArrayRank();
                    var elementType = type == typeof(object) ? typeof(object) : type.GetElementType();
                    var array = Array.CreateInstance(elementType, arrayValue.value.Length);
                    var elements = new object[arrayValue.value.Length];
                    for (int i = 0; i < arrayValue.value.Length; i++)
                    {
                        if (!ToObject(elementType, arrayValue.value[i], out object element, out IonConversionException e))
                        {
                            return Reject(e, out result, out exception);
                        }
                        elements[i] = element;
                    }
                    FillMultiDimArray(array, elements, arrayValue.value.Length);
                    return Accept(array, out result, out exception);
                }
                else if (TypeUtils.IsCollection(type, out var elementType))
                {
                    if (!TypeUtils.TryCreateInstance(type, out object instance))
                    {
                        return Reject(new TypeMismatchException("Failed to create instance of " + type.Name), out result, out exception);
                    }
                    var wrapper = new CollectionWrapper(type, elementType, instance);
                    foreach (var element in arrayValue.value)
                    {
                        if (!ToObject(elementType, element, out object elementObject, out IonConversionException e))
                        {
                            return Reject(e, out result, out exception);
                        }
                        wrapper.Add(elementObject);
                    }
                    return Accept(wrapper.GetCollection(), out result, out exception);
                }
                return Reject(new TypeMismatchException("Failed to convert array to " + type.Name), out result, out exception);
            }
            else if (value is MapValue mapValue)
            {
                if (type == typeof(object))
                {
                    var instance = new Dictionary<string, object>();
                    foreach (var pair in mapValue.value)
                    {
                        if (!ToObject(typeof(object), pair.Value, out object pairValue, out IonConversionException e))
                        {
                            return Reject(e, out result, out exception);
                        }
                        instance.Add(pair.Key, pairValue);
                    }
                    return Accept(instance, out result, out exception);
                }
                else if (type.IsClass || TypeUtils.IsStruct(type))
                {
                    if (!TypeUtils.TryCreateInstance(type, out object instance))
                    {
                        return Reject(new TypeMismatchException("Failed to create instance of " + type.Name), out result, out exception);
                    }
                    var unusedKeys = new HashSet<string>(mapValue.value.Keys);
                    foreach (var field in TypeUtils.GetFields(type))
                    {
                        string fieldName = field.Name;
                        if (!mapValue.value.TryGetValue(fieldName, out IIonValue fieldValue))
                        {
                            if (fromIonSettings.useDefaultSetting == FromIonSettings.UseDefaultSetting.ForMissingFields
                                || fromIonSettings.useDefaultSetting == FromIonSettings.UseDefaultSetting.ForMissingOrMismatchedFields)
                            {
                                field.SetValue(instance, TypeUtils.DefaultValue(type));
                                unusedKeys.Remove(fieldName);
                                continue;
                            }
                            return Reject(new TypeMismatchException("Failed to convert map to " + type.Name + ": missing field " + fieldName), out result, out exception);
                        }
                        if (!ToObject(field.FieldType, fieldValue, out object fieldValueObject, out IonConversionException e))
                        {
                            if (fromIonSettings.useDefaultSetting == FromIonSettings.UseDefaultSetting.ForMissingOrMismatchedFields)
                            {
                                field.SetValue(instance, TypeUtils.DefaultValue(type));
                                unusedKeys.Remove(fieldName);
                                continue;
                            }
                            return Reject(e, out result, out exception);
                        }
                        field.SetValue(instance, fieldValueObject);
                        unusedKeys.Remove(fieldName);
                    }
                    if (!fromIonSettings.ignoreExtraFields && unusedKeys.Count > 0)
                    {
                        return Reject(new TypeMismatchException("Failed to convert map to " + type.Name + ": extra fields"), out result, out exception);
                    }
                    return Accept(instance, out result, out exception);
                }
                return Reject(new TypeMismatchException("Failed to convert map to " + type.Name), out result, out exception);
            }
            else
            {
                return Reject(new InternalException("Unknown IonValue Type" + type.Name), out result, out exception);
            }
        }

        private static void FillMultiDimArray(Array array, object[] values, int[] dimensions)
        {
            int index = 0;
            int[] indices = new int[array.Rank];

            while (true)
            {
                // Set the value in the multidimensional array
                array.SetValue(values[index], indices);
                index++;
                if (index >= values.Length)
                    break;

                // Increment the indices
                for (int i = indices.Length - 1; i >= 0; i--)
                {
                    indices[i]++;
                    if (indices[i] < dimensions[i])
                        break;
                    if (i == 0)
                        return; // Finished filling the array
                    indices[i] = 0;
                }
            }
        }

        private IIonValue ConvertMultiDimArray(Array multiDimArray, int dimension, int depth)
        {
            int length = multiDimArray.GetLength(dimension);

            // Create an array of arrays (jagged array) for the current dimension
            IIonValue[] jaggedArray = new IIonValue[length];

            if (dimension == multiDimArray.Rank - 1)
            {
                for (int i = 0; i < length; i++)
                {
                    int[] indices = new int[multiDimArray.Rank];
                    indices[dimension] = i;
                    if (!ToIonValue(multiDimArray.GetValue(indices), out var ele, out var ex, depth + 1))
                    {
                        throw ex;
                    }
                    jaggedArray[i] = ele;
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    int[] indices = new int[multiDimArray.Rank];
                    indices[dimension] = i;

                    // Create a sub-array for the next dimension
                    IIonValue subArray = ConvertMultiDimArray(multiDimArray, dimension + 1, depth);
                    jaggedArray.SetValue(subArray, i);
                }
            }

            return new ArrayValue(jaggedArray);
        }
    }
}
