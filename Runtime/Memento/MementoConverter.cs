using System;
using System.Collections;
using System.Collections.Generic;
using Yeast.Utils;

namespace Yeast.Memento
{
    public interface ICustomTransformer
    {
        object Serialize(object value);
        object Deserialize(object memento);

        public static Dictionary<Type, (Type, ICustomTransformer)> customTransformations = new()
        {
            { typeof(Guid), (typeof(string), new CustomTransformer<Guid, string>(g => g.ToString(), (s) => new Guid(s)) )},
            { typeof(DateTime), (typeof(long), new CustomTransformer<DateTime, long>(d => d.Ticks, l => new DateTime(l)) )},
            { typeof(TimeSpan), (typeof(long), new CustomTransformer<TimeSpan, long>(t => t.Ticks, l => new TimeSpan(l)) )},
            { typeof(Uri), (typeof(string), new CustomTransformer<Uri, string>(u => u.ToString(), s => new Uri(s)) )},
        };

        public static (Type, object) SerializeTransform(Type type, object obj)
        {
            if (customTransformations.TryGetValue(type, out (Type type, ICustomTransformer transformer) e))
            {
                return (e.type, e.transformer.Serialize(obj));
            }
            return (type, obj);
        }

        public static (Type, Func<object, object>) DeserializeTransformer(Type type)
        {
            if (customTransformations.TryGetValue(type, out (Type type, ICustomTransformer transformer) e))
            {
                return (e.type, obj => e.transformer.Deserialize(obj));
            }
            return (type, e => e);
        }

        public static Type GetDeserializationType(Type type)
        {
            if (customTransformations.TryGetValue(type, out (Type type, ICustomTransformer transformer) e))
            {
                return e.type;
            }
            return type;
        }
    }

    public class CustomTransformer<TFrom, TTo> : ICustomTransformer
    {
        public delegate TTo SerializeDelegate(TFrom value);
        public delegate TFrom DeserializeDelegate(TTo memento);

        private readonly SerializeDelegate serializer;
        private readonly DeserializeDelegate deserializer;

        public CustomTransformer(SerializeDelegate serializer, DeserializeDelegate deserializer)
        {
            this.serializer = serializer;
            this.deserializer = deserializer;
        }

        public object Serialize(object value)
        {
            return serializer((TFrom)value);
        }

        public object Deserialize(object memento)
        {
            return deserializer((TTo)memento);
        }
    }

    public class MementoConversionException : Exception
    {
        public MementoConversionException(string message) : base(message) { }
    }

    public class TypeMismatchException : MementoConversionException
    {
        public TypeMismatchException(string message) : base(message) { }
    }

    public class CircularReferenceException : MementoConversionException
    {
        public CircularReferenceException(string message) : base(message) { }
    }

    public class MementoConverter
    {
        public IMemento Serialize(object value)
        {
            var visitor = new SerializationMementoVisitor(1000);
            return visitor.Serialize(value);
        }

        public object Deserialize(Type type, IMemento memento)
        {
            var visitor = new DeserializationMementoVisitor();
            return visitor.Deserialize(type, memento);
        }
    }
    public class DeserializationMementoVisitor : IMementoVisitor
    {
        private TypeWrapper type;
        public object result;

        public object Deserialize(Type type, IMemento memento)
        {
            if (type == typeof(object))
            {
                return memento.GetValueAsObject();
            }

            var oldType = this.type;
            var (newType, transformer) = ICustomTransformer.DeserializeTransformer(type);
            this.type = TypeWrapper.FromType(newType);

            memento.Accept(this);
            this.type = oldType;
            return transformer(result);
        }

        public void Visit(NullMemento memento)
        {
            if (type.IsNullable)
            {
                result = null;
            }
            else throw new TypeMismatchException("Failed to convert null to " + type.Type.Name + ": type not nullable");
        }

        public void Visit(StringMemento memento)
        {
            if (type is StringTypeWrapper)
            {
                result = memento.value;
            }
            else if (type is RuntimeTypeTypeWrapper)
            {
                result = Type.GetType(memento.value);
            }
            else throw new TypeMismatchException("Failed to convert string to " + type.Type.Name);
        }

        public void Visit(IntegerMemento memento)
        {
            if (type is IntegerTypeWrapper intType)
            {
                if (intType.IsEnum)
                {
                    result = Enum.ToObject(intType.Type, memento.value);
                }
                else
                {
                    result = Convert.ChangeType(memento.value, intType.Type);
                }
            }
            else throw new TypeMismatchException("Failed to convert long to " + type.Type.Name);
        }

        public void Visit(DecimalMemento memento)
        {
            if (type is RationalTypeWrapper ratType)
            {
                result = Convert.ChangeType(memento.value, ratType.Type);
            }
            else throw new TypeMismatchException("Failed to convert double to " + type.Type.Name);
        }

        public void Visit(BoolMemento memento)
        {
            if (type is BoolTypeWrapper)
            {
                result = memento.value;
            }
            else throw new TypeMismatchException("Failed to convert bool to " + type.Type.Name);
        }

        public void Visit(ArrayMemento memento)
        {
            if (type is CollectionTypeWrapper colType)
            {
                var elementType = colType.ElementType.Type;
                if (colType.IsCollection)
                {
                    if (!TypeUtils.TryCreateInstance(colType.Type, out object instance))
                    {
                        throw new TypeMismatchException("Failed to create instance of " + type.Type.Name);
                    }
                    var wrapper = new CollectionWrapper(elementType, instance);
                    foreach (var element in memento.value)
                    {
                        var elementObject = Deserialize(elementType, element);
                        wrapper.Add(elementObject);
                    }
                    result = wrapper.GetCollection();
                }
                else
                {
                    object Transform(IMemento val) => Deserialize(elementType, val);

                    var arr = ArrayUtils.MementoToArray(elementType, colType.Type, Transform, memento);
                    result = arr;
                }
            }
            else throw new TypeMismatchException("Failed to convert array to " + type.Type.Name);
        }

        public void Visit(DictMemento memento)
        {
            if (type is StructTypeWrapper structType)
            {
                string typeID = memento.value.ContainsKey("$type") ? ((StringMemento)memento.value["$type"]).value : null;
                memento.value.Remove("$type");

                var usedType = (typeID != null && structType.DerivedTypes.TryGetValue(typeID, out var derivedType)) ? derivedType : structType;

                if (!usedType.IsInstantiable)
                {
                    throw new TypeMismatchException("Failed to convert map to " + usedType.Type.Name + ": type is not instantiateable");
                }

                if (!TypeUtils.TryCreateInstance(usedType.Type, out object instance))
                {
                    throw new TypeMismatchException("Failed to create instance of " + usedType.Type.Name);
                }
                foreach (var (fieldName, field) in usedType.Fields)
                {
                    if (!memento.value.TryGetValue(fieldName, out IMemento fieldValue))
                    {
                        field.setter(instance, TypeUtils.DefaultValue(usedType.Type));
                        continue;
                    }

                    object fieldValueObject = Deserialize(field.type.FullType, fieldValue);
                    field.setter(instance, fieldValueObject);
                }
                result = instance;
            }
            else throw new TypeMismatchException("Failed to convert map to " + type.Type.Name);
        }
    }

    public class SerializationMementoVisitor : ITypeWrapperVisitor
    {
        private object value;
        private IMemento result;
        private int depth;

        public SerializationMementoVisitor(int maxDepth)
        {
            depth = maxDepth;
        }

        public IMemento Serialize(object value)
        {
            if (value is null)
            {
                return new NullMemento();
            }

            var oldValue = this.value;
            this.value = value;

            depth--;
            if (depth < 0)
            {
                throw new CircularReferenceException("Recursion limit reached. Your object may contain circular references.");
            }

            var type = value is Type ? typeof(Type) : value.GetType();
            (type, this.value) = ICustomTransformer.SerializeTransform(type, value);

            var typeWrapper = TypeWrapper.FromType(type);
            typeWrapper.Accept(this);

            depth++;
            this.value = oldValue;
            return result;
        }

        public void Visit(StringTypeWrapper stringTypeWrapper)
        {
            result = new StringMemento((string)value);
        }

        public void Visit(BoolTypeWrapper boolTypeWrapper)
        {
            if (value is null && boolTypeWrapper.IsNullable)
            {
                result = new NullMemento();
                return;
            }
            result = new BoolMemento((bool)value);
        }

        public void Visit(IntegerTypeWrapper integerTypeWrapper)
        {
            if (value is null && integerTypeWrapper.IsNullable)
            {
                result = new NullMemento();
                return;
            }
            result = new IntegerMemento(Convert.ToInt64(value));
        }

        public void Visit(RationalTypeWrapper rationalTypeWrapper)
        {
            if (value is null && rationalTypeWrapper.IsNullable)
            {
                result = new NullMemento();
                return;
            }
            result = new DecimalMemento(Convert.ToDouble(value));
        }

        public void Visit(CollectionTypeWrapper collectionTypeWrapper)
        {
            if (value is null)
            {
                result = new NullMemento();
                return;
            }

            if (collectionTypeWrapper.IsCollection)
            {
                var intermediateArray = new List<IMemento>();
                var collection = (IEnumerable)value;
                foreach (var element in collection)
                {
                    var el = Serialize(element);
                    intermediateArray.Add(el);
                }
                result = new ArrayMemento(intermediateArray);
            }
            else
            {
                result = ArrayUtils.ArrayToMemento(Serialize, (Array)value);
            }
        }

        public void Visit(StructTypeWrapper structTypeWrapper)
        {
            if (value is null && structTypeWrapper.IsNullable)
            {
                result = new NullMemento();
                return;
            }

            var obj = new Dictionary<string, IMemento>();

            if (structTypeWrapper.TypeIdentifier is not null)
            {
                obj.Add("$type", new StringMemento(structTypeWrapper.TypeIdentifier));
            }

            foreach (var (fieldName, field) in structTypeWrapper.Fields)
            {
                obj.Add(fieldName, Serialize(field.getter(value)));
            }

            result = new DictMemento(obj);
        }

        public void Visit(RuntimeTypeTypeWrapper typeWrapper)
        {
            result = new StringMemento(((Type)value).AssemblyQualifiedName);
        }
    }
}