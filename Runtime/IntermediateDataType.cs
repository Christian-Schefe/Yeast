using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public interface IConverter<K>
{
    public K Stringify(SimpleValue value);
    public SimpleValue Parse(Type type, K data);
    public SimpleValue Parse<T>(K data)
    {
        return Parse(typeof(T), data);
    }
}

public abstract class SimpleValue
{
    public static SimpleValue From(object value)
    {
        if (value == null)
        {
            return new NullValue();
        }
        Type type = value.GetType();
        if (value is bool boolValue)
        {
            return new BooleanValue { value = boolValue };
        }
        else if (value is string stringValue)
        {
            return new StringValue { value = stringValue };
        }
        else if (TypeUtils.IsIntegerNumber(type) || type == typeof(char) || type.IsEnum)
        {
            long val = Convert.ToInt64(value);
            return new IntegerValue { value = val };
        }
        else if (TypeUtils.IsRationalNumber(type))
        {
            double val = Convert.ToDouble(value);
            return new FloatValue { value = val };
        }
        else if (TypeUtils.IsCollection(type, out _))
        {
            IEnumerable collection = (IEnumerable)value;
            var intermediateArray = new List<SimpleValue>();
            foreach (var element in collection)
            {
                intermediateArray.Add(From(element));
            }
            return new ArrayValue { value = intermediateArray };
        }
        else
        {
            var intermediateMap = new Dictionary<string, SimpleValue>();
            var fields = TypeUtils.GetFields(type);
            foreach (var field in fields)
            {
                intermediateMap.Add(field.Name, From(field.GetValue(value)));
            }
            return new MapValue { value = intermediateMap };
        }
    }

    public bool TryInto(Type type, out object value, out Exception exception)
    {
        if (type == null)
        {
            throw new ArgumentNullException("type");
        }
        else if (!TypeUtils.IsInstantiateable(type) || type.IsSubclassOf(typeof(UnityEngine.Object)))
        {
            throw new ConversionExceptions.InvalidTypeException("Cannot deserialize to new instances of type '" + type.Name + ".'");
        }
        return TryIntoInternal(type, out value, out exception);
    }

    protected abstract bool TryIntoInternal(Type type, out object value, out Exception exception);

    public class StringValue : SimpleValue
    {
        public string value;

        protected override bool TryIntoInternal(Type type, out object value, out Exception exception)
        {
            if (type == typeof(string) || type == typeof(object))
            {
                value = this.value;
                exception = null;
                return true;
            }
            value = default(string);
            exception = new ConversionExceptions.InvalidCastException("Failed to convert string to " + type.Name);
            return false;
        }
    }

    public class IntegerValue : SimpleValue
    {
        public long value;

        protected override bool TryIntoInternal(Type type, out object value, out Exception exception)
        {
            if (TypeUtils.IsIntegerNumber(type) || TypeUtils.IsRationalNumber(type))
            {
                try
                {
                    value = Convert.ChangeType(this.value, type);
                    exception = null;
                    return true;
                }
                catch
                {
                    value = default(long);
                    exception = new ConversionExceptions.InvalidCastException("Failed to convert long to " + type.Name + ": cast failed");
                    return false;
                }
            }
            else if (type == typeof(object))
            {
                value = this.value;
                exception = null;
                return true;
            }
            value = default(long);
            exception = new ConversionExceptions.InvalidCastException("Failed to convert long to " + type.Name + ": not a number type");
            return false;
        }
    }

    public class FloatValue : SimpleValue
    {
        public double value;

        protected override bool TryIntoInternal(Type type, out object value, out Exception exception)
        {
            if (TypeUtils.IsRationalNumber(type))
            {
                try
                {
                    value = Convert.ChangeType(this.value, type);
                    exception = null;
                    return true;
                }
                catch
                {
                    value = default(double);
                    exception = new ConversionExceptions.InvalidCastException("Failed to convert double to " + type.Name + ": cast failed");
                    return false;
                }
            }
            else if (type == typeof(object))
            {
                value = this.value;
                exception = null;
                return true;
            }
            value = default(double);
            exception = new ConversionExceptions.InvalidCastException("Failed to convert double to " + type.Name + ": not a number type");
            return false;
        }
    }

    public class BooleanValue : SimpleValue
    {
        public bool value;

        protected override bool TryIntoInternal(Type type, out object value, out Exception exception)
        {
            if (type == typeof(bool))
            {
                value = this.value;
                exception = null;
                return true;
            }
            else if (type == typeof(object))
            {
                value = this.value;
                exception = null;
                return true;
            }
            value = default(bool);
            exception = new ConversionExceptions.InvalidCastException("Failed to convert bool to " + type.Name);
            return false;
        }
    }

    public class ArrayValue : SimpleValue
    {
        public List<SimpleValue> value;

        protected override bool TryIntoInternal(Type type, out object value, out Exception exception)
        {
            if (type == typeof(object) || type.IsArray)
            {
                var elementType = type == typeof(object) ? typeof(object) : type.GetElementType();
                var array = Array.CreateInstance(elementType, this.value.Count);
                for (int i = 0; i < this.value.Count; i++)
                {
                    if (!this.value[i].TryInto(elementType, out object element, out Exception e))
                    {
                        value = default;
                        exception = e;
                        return false;
                    }
                    array.SetValue(element, i);
                }
                value = array;
                exception = null;
                return true;
            }
            else if (TypeUtils.IsCollection(type, out var elementType))
            {
                if (!TypeUtils.TryCreateInstance(type, out object instance))
                {
                    value = null;
                    exception = new ConversionExceptions.InvalidTypeException("Failed to create instance of " + type.Name);
                    return false;
                }
                var collection = new CollectionWrapper(type, elementType, instance);
                foreach (var element in this.value)
                {
                    if (!element.TryInto(elementType, out object elementObject, out Exception e))
                    {
                        value = default;
                        exception = e;
                        return false;
                    }
                    collection.Add(elementObject);
                }
                value = collection.GetCollection();
                exception = null;
                return true;
            }
            value = default;
            exception = new ConversionExceptions.InvalidTypeException("Failed to convert array to " + type.Name + ": not a collection type");
            return false;
        }
    }

    public class MapValue : SimpleValue
    {
        public Dictionary<string, SimpleValue> value;

        protected override bool TryIntoInternal(Type type, out object value, out Exception exception)
        {
            if (type == typeof(object))
            {
                var instance = new Dictionary<string, object>();
                foreach (var pair in this.value)
                {
                    if (!pair.Value.TryInto(typeof(object), out object pairValue, out Exception e))
                    {
                        value = default;
                        exception = e;
                        return false;
                    }
                    instance.Add(pair.Key, pairValue);
                }
                value = instance;
                exception = null;
                return true;
            }
            else if (type.IsClass || TypeUtils.IsStruct(type))
            {
                if (!TypeUtils.TryCreateInstance(type, out object instance))
                {
                    value = null;
                    exception = new ConversionExceptions.InvalidTypeException("Failed to create instance of " + type.Name);
                    return false;
                }
                foreach (var field in TypeUtils.GetFields(type))
                {
                    if (!this.value.TryGetValue(field.Name, out SimpleValue fieldValue))
                    {
                        value = default;
                        exception = new ConversionExceptions.InvalidTypeException("Failed to convert map to " + type.Name + ": missing field " + field.Name);
                        return false;
                    }
                    if (!fieldValue.TryInto(field.FieldType, out object fieldValueObject, out Exception e))
                    {
                        value = default;
                        exception = e;
                        return false;
                    }
                    field.SetValue(instance, fieldValueObject);
                }
                value = instance;
                exception = null;
                return true;
            }
            value = default;
            exception = new ConversionExceptions.InvalidTypeException("Failed to convert map to " + type.Name + ": not a class type");
            return false;
        }
    }

    public class NullValue : SimpleValue
    {
        protected override bool TryIntoInternal(Type type, out object value, out Exception exception)
        {
            if (type == typeof(object) || type.IsClass || Nullable.GetUnderlyingType(type) != null)
            {
                value = null;
                exception = null;
                return true;
            }
            value = default;
            exception = new ConversionExceptions.InvalidCastException("Failed to convert null to " + type.Name + ": type not nullable");
            return false;
        }
    }
}

public static class ConversionExceptions
{
    public class ConversionException : Exception
    {
        public ConversionException(string message) : base(message) { }
    }

    public class InvalidCastException : ConversionException
    {
        public InvalidCastException(string message) : base(message) { }
    }

    public class InvalidTypeException : ConversionException
    {
        public InvalidTypeException(string message) : base(message) { }
    }
}

public static class TypeUtils
{
    public static bool IsInstantiateable(Type type)
    {
        if (type == null)
        {
            return false;
        }
        else if (type.IsInterface || type.IsAbstract || type.ContainsGenericParameters)
        {
            return false;
        }
        else if (type.IsValueType || type == typeof(string) || type.IsArray)
        {
            return true;
        }
        else if (type.IsClass && HasDefaultConstructor(type))
        {
            return true;
        }
        return false;
    }

    public static bool HasDefaultConstructor(Type type)
    {
        return type.GetConstructor(Type.EmptyTypes) != null;
    }

    public static bool TryCreateInstance(Type type, out object instance)
    {
        try
        {
            instance = Activator.CreateInstance(type);
            return true;
        }
        catch
        {
            instance = null;
            return false;
        }
    }

    public static bool IsCollection(Type type, out Type elementType)
    {
        var result = ImplementsGenericInterface(type, typeof(ICollection<>), out var genericArguments);
        elementType = result ? genericArguments[0] : null;
        return result;
    }

    public static bool IsEnumerable(Type type)
    {
        return ImplementsInterface(type, typeof(IEnumerable));
    }

    public static bool ImplementsInterface(Type type, Type interfaceType)
    {
        foreach (var t in type.GetInterfaces())
        {
            if (t == interfaceType)
            {
                return true;
            }
        }
        return false;
    }

    public static bool ImplementsGenericInterface(Type type, Type interfaceType, out Type[] genericArguments)
    {
        foreach (var t in type.GetInterfaces())
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType)
            {
                genericArguments = t.GetGenericArguments();
                return true;
            }
        }
        genericArguments = null;
        return false;
    }

    public static bool IsNullable(Type type)
    {
        return type.IsClass || Nullable.GetUnderlyingType(type) != null;
    }

    public static bool IsStruct(Type type)
    {
        return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
    }

    public static bool IsIntegerNumber(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(byte) || type == typeof(short);
    }

    public static bool IsRationalNumber(Type type)
    {
        return type == typeof(float) || type == typeof(double);
    }

    public static FieldInfo[] GetFields(Type type)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var serializableFields = new List<FieldInfo>();
        foreach (var field in fields)
        {
            if (field.GetCustomAttribute(typeof(NonSerializedAttribute)) == null)
            {
                serializableFields.Add(field);
            }
        }
        return serializableFields.ToArray();
    }

    public static PropertyInfo[] GetWritableProperties(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var writableProperties = new List<PropertyInfo>();
        foreach (var property in properties)
        {
            if (property.CanWrite)
            {
                writableProperties.Add(property);
            }
        }
        return writableProperties.ToArray();
    }
}

public class CollectionWrapper
{
    private readonly Action<object> addMethod;
    private readonly object collection;

    public CollectionWrapper(Type collectionType, Type elementType, object collection)
    {
        this.collection = collection;
        try
        {
            var delegateCreator = typeof(CollectionWrapper).GetMethod(nameof(GetAddDelegate), BindingFlags.NonPublic | BindingFlags.Static);
            var genericDelegateCreator = delegateCreator.MakeGenericMethod(collectionType, elementType);
            addMethod = (Action<object>)genericDelegateCreator.Invoke(null, new object[] { collection });
        }
        catch (Exception e)
        {
            throw new Exception("Failed to cache delegate: " + e.Message);
        }
    }

    private static Action<object> GetAddDelegate<T, K>(T collection) where T : ICollection<K>
    {
        return e => collection.Add((K)e);
    }

    public void Add(object value)
    {
        addMethod(value);
    }

    public object GetCollection()
    {
        return collection;
    }
}