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
            bool isNullable = TypeUtils.IsNullable(type, out var baseType);

            if (StringTypeWrapper.Matches(baseType)) return new StringTypeWrapper(baseType);
            else if (BoolTypeWrapper.Matches(baseType)) return new BoolTypeWrapper(baseType, isNullable);
            else if (IntegerTypeWrapper.Matches(baseType)) return new IntegerTypeWrapper(baseType, isNullable);
            else if (RationalTypeWrapper.Matches(baseType)) return new RationalTypeWrapper(baseType, isNullable);
            else if (CollectionTypeWrapper.Matches(baseType)) return new CollectionTypeWrapper(baseType);
            else if (StructTypeWrapper.Matches(baseType)) return new StructTypeWrapper(baseType, isNullable);
            else throw new InvalidOperationException($"Cannot create TypeWrapper from type {baseType}");
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
        public IntegerTypeWrapper(Type type, bool isNullable) : base(type, isNullable)
        {
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

        public int Rank => rank;
        public TypeWrapper ElementType => elementType;

        public CollectionTypeWrapper(Type type) : base(type, true)
        {
            if (TypeUtils.IsCollection(type, out var elType))
            {
                rank = 1;
                elementType = FromType(elType);
            }
            else
            {
                rank = type.GetArrayRank();
                elementType = FromType(type.GetElementType());
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

        public string TypeIdentifier => typeIdentifier;
        public Dictionary<string, StructTypeWrapper> DerivedTypes => derivedTypes;
        public Dictionary<string, Field> Fields => fields;

        public StructTypeWrapper(Type type, bool isNullable) : base(type, isNullable)
        {
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
            public TypeWrapper type;
            public string name;
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
    }

    public abstract class TypeWrapperVisitor<TIn, TOut> : ITypeWrapperVisitor
    {
        protected TIn value;
        protected TOut result;

        public TOut Convert(TIn value, TypeWrapper type)
        {
            var customType = ICustomTransformer.GetDeserializationType(type.Type);
            type = TypeWrapper.FromType(customType);

            this.value = value;
            type.Accept(this);
            return result;
        }

        public abstract void Visit(StringTypeWrapper stringTypeWrapper);
        public abstract void Visit(BoolTypeWrapper boolTypeWrapper);
        public abstract void Visit(IntegerTypeWrapper integerTypeWrapper);
        public abstract void Visit(RationalTypeWrapper rationalTypeWrapper);
        public abstract void Visit(CollectionTypeWrapper collectionTypeWrapper);
        public abstract void Visit(StructTypeWrapper structTypeWrapper);
    }
}
