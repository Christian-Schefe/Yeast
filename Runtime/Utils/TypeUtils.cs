using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Yeast.Utils
{
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

        public static bool IsNullable(Type type, out Type baseType)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            baseType = underlyingType ?? type;
            return type.IsClass || underlyingType != null;
        }

        public static bool IsStruct(Type type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }

        public static bool IsIntegerNumber(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(byte) || type == typeof(short) || type == typeof(sbyte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);
        }

        public static bool IsRationalNumber(Type type)
        {
            return type == typeof(float) || type == typeof(double);
        }

        public static object DefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static FieldInfo[] GetFields(Type type)
        {
            var serializableFields = new List<FieldInfo>();

            void AddFields(Type type)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    if (field.DeclaringType == type && field.GetCustomAttribute(typeof(NonSerializedAttribute)) == null)
                    {
                        serializableFields.Add(field);
                    }
                }
                var baseType = type.BaseType;
                if (baseType != null) AddFields(baseType);
            }

            AddFields(type);
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

        public static bool HasAttribute<T>(Type type, out T attribute) where T : Attribute
        {
            var attrs = type.GetCustomAttribute(typeof(T), false);
            if (attrs != null)
            {
                attribute = (T)attrs;
                return true;
            }
            attribute = default;
            return false;
        }

        public static bool TryGetDerivedTypeById(Type type, string typeIdentifier, out Type derivedType)
        {
            derivedType = null;
            if (!HasAttribute(type, out HasDerivedClassesAttribute attr)) return false;

            foreach (var t in attr.DerivedTypes)
            {
                if (!HasAttribute<IsDerivedClassAttribute>(t, out var derivedAttr) || derivedAttr.Identifier != typeIdentifier) continue;

                derivedType = t;
                return true;
            }
            return false;
        }
    }
}
