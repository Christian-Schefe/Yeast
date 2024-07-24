using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Yeast.Utils;

namespace Yeast.Memento
{
    public interface ITypeWrapper
    {
        public void Accept(ITypeWrapperVisitor visitor);
    }

    public abstract class TypeWrapper : ITypeWrapper
    {
        private static readonly Dictionary<Type, TypeWrapper> typeWrappers = new();

        protected Type type;
        protected bool isNullable;

        public Type Type => type;
        public bool IsNullable => isNullable;

        public TypeWrapper(Type type, bool isNullable)
        {
            this.type = type;
            this.isNullable = isNullable;
        }

        public abstract void Accept(ITypeWrapperVisitor visitor);

        public static TypeWrapper FromType(Type type)
        {
            if (typeWrappers.TryGetValue(type, out var wrapper)) return wrapper;

            bool isNullable = TypeUtils.IsNullable(type, out var baseType);

            TypeWrapper result;

            if (StringTypeWrapper.Matches(baseType)) result = new StringTypeWrapper(baseType);
            else if (BoolTypeWrapper.Matches(baseType)) result = new BoolTypeWrapper(baseType, isNullable);
            else if (IntegerTypeWrapper.Matches(baseType)) result = new IntegerTypeWrapper(baseType, isNullable);
            else if (RationalTypeWrapper.Matches(baseType)) result = new RationalTypeWrapper(baseType, isNullable);
            else if (RuntimeTypeTypeWrapper.Matches(baseType)) result = new RuntimeTypeTypeWrapper(baseType);
            else if (CollectionTypeWrapper.Matches(baseType)) result = new CollectionTypeWrapper(baseType);
            else if (StructTypeWrapper.Matches(baseType)) result = new StructTypeWrapper(baseType, isNullable);
            else throw new InvalidOperationException($"Cannot create TypeWrapper from type {baseType}");

            typeWrappers.Add(type, result);
            return result;
        }
    }

    public class StringTypeWrapper : TypeWrapper
    {
        public StringTypeWrapper(Type type) : base(type, true)
        {
        }

        public override void Accept(ITypeWrapperVisitor visitor)
        {
            visitor.Visit(this);
        }

        public static bool Matches(Type type)
        {
            return type == typeof(string);
        }
    }

    public class BoolTypeWrapper : TypeWrapper
    {
        public BoolTypeWrapper(Type type, bool isNullable) : base(type, isNullable)
        {
        }

        public override void Accept(ITypeWrapperVisitor visitor)
        {
            visitor.Visit(this);
        }

        public static bool Matches(Type type)
        {
            return type == typeof(bool);
        }
    }

    public class IntegerTypeWrapper : TypeWrapper
    {
        private bool isEnum;

        public bool IsEnum => isEnum;

        public IntegerTypeWrapper(Type type, bool isNullable) : base(type, isNullable)
        {
            isEnum = type.IsEnum;
        }

        public override void Accept(ITypeWrapperVisitor visitor)
        {
            visitor.Visit(this);
        }

        public static bool Matches(Type type)
        {
            return TypeUtils.IsIntegerNumber(type) || type.IsEnum || type == typeof(char);
        }
    }

    public class RationalTypeWrapper : TypeWrapper
    {
        public RationalTypeWrapper(Type type, bool isNullable) : base(type, isNullable)
        {
        }

        public override void Accept(ITypeWrapperVisitor visitor)
        {
            visitor.Visit(this);
        }

        public static bool Matches(Type type)
        {
            return TypeUtils.IsRationalNumber(type);
        }
    }

    public class CollectionTypeWrapper : TypeWrapper
    {
        private readonly int rank;
        private readonly TypeWrapper elementType;
        private readonly bool isCollection;

        public int Rank => rank;
        public TypeWrapper ElementType => elementType;
        public bool IsCollection => isCollection;

        public CollectionTypeWrapper(Type type) : base(type, true)
        {
            if (TypeUtils.IsCollection(type, out var elType))
            {
                rank = 1;
                elementType = FromType(elType);
                isCollection = !type.IsArray;
            }
            else
            {
                rank = type.GetArrayRank();
                elementType = FromType(type.GetElementType());
                isCollection = false;
            }
        }

        public override void Accept(ITypeWrapperVisitor visitor)
        {
            visitor.Visit(this);
        }

        public static bool Matches(Type type)
        {
            return type.IsArray || TypeUtils.IsCollection(type, out _);
        }
    }

    public class StructTypeWrapper : TypeWrapper
    {
        private readonly string typeIdentifier;
        private readonly Dictionary<string, StructTypeWrapper> derivedTypes;
        private readonly Dictionary<string, Field> fields;
        private readonly bool isInstantiable;

        public string TypeIdentifier => typeIdentifier;
        public Dictionary<string, StructTypeWrapper> DerivedTypes => derivedTypes;
        public Dictionary<string, Field> Fields => fields;
        public bool IsInstantiable => isInstantiable;

        public StructTypeWrapper(Type type, bool isNullable) : base(type, isNullable)
        {
            isInstantiable = TypeUtils.IsInstantiable(type);

            if (TypeUtils.HasAttribute(type, out IsDerivedClassAttribute attr))
            {
                typeIdentifier = attr.Identifier;
            }
            else typeIdentifier = null;

            derivedTypes = new();
            if (TypeUtils.HasAttribute(type, out HasDerivedClassesAttribute derivedAttr))
            {
                foreach (var derivedType in derivedAttr.DerivedTypes)
                {
                    if (!type.IsAssignableFrom(derivedType)) throw new InvalidOperationException($"Type {derivedType} is not derived from {type}");

                    if (TypeUtils.HasAttribute(derivedType, out IsDerivedClassAttribute derivedClassAttr))
                    {
                        derivedTypes.Add(derivedClassAttr.Identifier, (StructTypeWrapper)FromType(derivedType));
                    }
                }
            }

            var fieldArr = TypeUtils.GetFields(type);
            fields = new();
            foreach (var field in fieldArr)
            {
                var fieldType = field.FieldType;
                var fieldTypeWrapper = type == fieldType ? this : FromType(fieldType);

                fields.Add(field.Name, new Field
                {
                    setter = (obj, val) => field.SetValue(obj, val),
                    getter = obj => field.GetValue(obj),
                    type = fieldTypeWrapper,
                    name = field.Name
                });
            }
        }

        public override void Accept(ITypeWrapperVisitor visitor)
        {
            visitor.Visit(this);
        }

        public static bool Matches(Type type)
        {
            return type.IsClass || TypeUtils.IsStruct(type);
        }

        public struct Field
        {
            public Action<object, object> setter;
            public Func<object, object> getter;
            public TypeWrapper type;
            public string name;
        }
    }

    public class RuntimeTypeTypeWrapper : TypeWrapper
    {
        public RuntimeTypeTypeWrapper(Type type) : base(type, false)
        {
        }

        public override void Accept(ITypeWrapperVisitor visitor)
        {
            visitor.Visit(this);
        }

        public static bool Matches(Type type)
        {
            return typeof(Type).IsAssignableFrom(type);
        }
    }

    public interface ITypeWrapperVisitor
    {
        void Visit(StringTypeWrapper stringTypeWrapper);
        void Visit(BoolTypeWrapper boolTypeWrapper);
        void Visit(IntegerTypeWrapper integerTypeWrapper);
        void Visit(RationalTypeWrapper rationalTypeWrapper);
        void Visit(CollectionTypeWrapper collectionTypeWrapper);
        void Visit(StructTypeWrapper structTypeWrapper);
        void Visit(RuntimeTypeTypeWrapper runtimeTypeTypeWrapper);
    }

    public abstract class TypeWrapperVisitor<TIn, TOut> : ITypeWrapperVisitor
    {
        protected TIn value;
        protected TOut result;

        public TOut Convert(TIn value, TypeWrapper type)
        {
            var oldVal = this.value;
            var customType = ICustomTransformer.GetDeserializationType(type.Type);
            type = TypeWrapper.FromType(customType);

            this.value = value;
            type.Accept(this);
            this.value = oldVal;
            return result;
        }

        public abstract void Visit(StringTypeWrapper stringTypeWrapper);
        public abstract void Visit(BoolTypeWrapper boolTypeWrapper);
        public abstract void Visit(IntegerTypeWrapper integerTypeWrapper);
        public abstract void Visit(RationalTypeWrapper rationalTypeWrapper);
        public abstract void Visit(CollectionTypeWrapper collectionTypeWrapper);
        public abstract void Visit(StructTypeWrapper structTypeWrapper);
        public abstract void Visit(RuntimeTypeTypeWrapper runtimeTypeTypeWrapper);
    }
}
