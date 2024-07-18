using System;
using System.Collections;
using System.Collections.Generic;
using Yeast.Utils;

namespace Yeast.Memento
{
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
            return Serialize(value, 0);
        }

        private IMemento Serialize(object value, int depth)
        {
            if (depth > 1000)
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

                if (TypeUtils.HasAttribute(type, out IsDerivedClassAttribute attr))
                {
                    intermediateMap.Add("$type", new StringMemento(attr.Identifier));
                }

                return new DictMemento(intermediateMap);
            }
        }

        public object Deserialize(Type type, IMemento memento)
        {
            var visitor = new DeserializationMementoVisitor(this);
            visitor.Visit(memento, type);
            return visitor.result;
        }

        public class DeserializationMementoVisitor : IMementoVisitor
        {
            private readonly MementoConverter converter;
            private Type type;

            public object result;

            public DeserializationMementoVisitor(MementoConverter converter)
            {
                this.converter = converter;
            }

            public void Visit(IMemento memento, Type type)
            {
                this.type = type;
                memento.Accept(this);
            }

            public void Visit(NullMemento memento)
            {
                if (type.IsClass || Nullable.GetUnderlyingType(type) != null)
                {
                    result = null;
                }
                else throw new TypeMismatchException("Failed to convert null to " + type.Name + ": type not nullable");
            }

            public void Visit(StringMemento memento)
            {
                if (type == typeof(string) || type == typeof(object))
                {
                    result = memento.value;
                }
                else throw new TypeMismatchException("Failed to convert string to " + type.Name);
            }

            public void Visit(IntegerMemento memento)
            {
                if (type == typeof(object))
                {
                    result = memento.value;
                }
                else if (TypeUtils.IsIntegerNumber(type) || TypeUtils.IsRationalNumber(type))
                {
                    result = Convert.ChangeType(memento.value, type);
                }
                else if (type.IsEnum)
                {
                    result = Enum.ToObject(type, memento.value);
                }
                else throw new TypeMismatchException("Failed to convert long to " + type.Name);
            }

            public void Visit(DecimalMemento memento)
            {
                if (TypeUtils.IsIntegerNumber(type) || TypeUtils.IsRationalNumber(type))
                {
                    result = Convert.ChangeType(memento.value, type);
                }
                else if (type == typeof(object))
                {
                    result = memento.value;
                }
                else throw new TypeMismatchException("Failed to convert double to " + type.Name);
            }

            public void Visit(BoolMemento memento)
            {
                if (type == typeof(bool) || type == typeof(object))
                {
                    result = memento.value;
                }
                else throw new TypeMismatchException("Failed to convert bool to " + type.Name);
            }

            public void Visit(ArrayMemento memento)
            {
                if (type == typeof(object))
                {
                    var array = new object[memento.value.Length];
                    for (int i = 0; i < memento.value.Length; i++)
                    {
                        array[i] = converter.Deserialize(typeof(object), memento.value[i]);
                    }
                    result = array;
                }
                else if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    object Transform(IMemento val) => converter.Deserialize(elementType, val);

                    var arr = ArrayUtils.MementoToArray(elementType, type, Transform, memento);
                    result = arr;
                }
                else if (TypeUtils.IsCollection(type, out var elementType))
                {
                    if (!TypeUtils.TryCreateInstance(type, out object instance))
                    {
                        throw new TypeMismatchException("Failed to create instance of " + type.Name);
                    }
                    var wrapper = new CollectionWrapper(type, elementType, instance);
                    foreach (var element in memento.value)
                    {
                        var elementObject = converter.Deserialize(elementType, element);
                        wrapper.Add(elementObject);
                    }
                    result = wrapper.GetCollection();
                }
                else throw new TypeMismatchException("Failed to convert array to " + type.Name);
            }

            public void Visit(DictMemento memento)
            {
                if (type == typeof(object))
                {
                    var instance = new Dictionary<string, object>();
                    foreach (var pair in memento.value)
                    {
                        object pairValue = converter.Deserialize(typeof(object), pair.Value);
                        instance.Add(pair.Key, pairValue);
                    }
                    result = instance;
                }
                else if (type.IsClass || TypeUtils.IsStruct(type))
                {
                    string typeID = memento.value.ContainsKey("$type") ? ((StringMemento)memento.value["$type"]).value : null;

                    if (!TypeUtils.IsInstantiateable(type) && typeID == null)
                    {
                        throw new TypeMismatchException("Failed to convert map to " + type.Name + ": type is not instantiateable and there is no type ID");
                    }

                    if (typeID != null)
                    {
                        bool hasDerivedClassAttribute = TypeUtils.HasAttribute(type, out HasDerivedClassesAttribute attr);
                        if (!hasDerivedClassAttribute)
                        {
                            throw new TypeMismatchException("Failed to convert map to " + type.Name + ": type does not have a HasDerivedClassesAttribute");
                        }
                        Type derivedType = null;
                        foreach (var t in attr.DerivedTypes)
                        {
                            if (TypeUtils.HasAttribute(t, out IsDerivedClassAttribute derivedAttr) && derivedAttr.Identifier == typeID)
                            {
                                derivedType = t;
                                break;
                            }
                        }

                        if (derivedType is null)
                        {
                            throw new TypeMismatchException("Failed to convert map to " + type.Name + ": type ID does not match any derived class");
                        }
                        if (!type.IsAssignableFrom(derivedType))
                        {
                            throw new TypeMismatchException("Failed to convert map to " + type.Name + $": {derivedType.Name} is not a derived type");
                        }
                        memento.value.Remove("$type");
                        Visit(new DictMemento(memento.value), derivedType);
                    }
                    else
                    {
                        if (!TypeUtils.TryCreateInstance(type, out object instance))
                        {
                            throw new TypeMismatchException("Failed to create instance of " + type.Name);
                        }
                        foreach (var field in TypeUtils.GetFields(type))
                        {
                            string fieldName = field.Name;
                            if (!memento.value.TryGetValue(fieldName, out IMemento fieldValue))
                            {
                                field.SetValue(instance, TypeUtils.DefaultValue(type));
                                continue;
                            }

                            try
                            {
                                object fieldValueObject = converter.Deserialize(field.FieldType, fieldValue);
                                field.SetValue(instance, fieldValueObject);
                            }
                            catch (TypeMismatchException)
                            {
                                field.SetValue(instance, TypeUtils.DefaultValue(type));
                                continue;
                            }
                        }
                        result = instance;
                    }
                }
                else throw new TypeMismatchException("Failed to convert map to " + type.Name);
            }
        }
    }
}
