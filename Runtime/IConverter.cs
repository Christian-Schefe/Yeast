using System;
using Yeast.Memento;

namespace Yeast
{
    public interface IMementoSerializer<out T, Settings>
    {
        public T Serialize(IMemento value, Settings settings);
    }

    public interface IMementoDeserializer<T, Settings>
    {
        public IMemento Deserialize(T value, Settings settings);
    }

    public interface IMementoConverter<T, SerializationSettings, DeserializationSettings> : IMementoSerializer<T, SerializationSettings>, IMementoDeserializer<T, DeserializationSettings>
    {
    }

    public abstract class BaseMementoConverter<T, SerializationSettings, DeserializationSettings> : IMementoConverter<T, SerializationSettings, DeserializationSettings>
    {
        protected SerializationSettings serializationSettings;
        protected DeserializationSettings deserializationSettings;

        protected abstract T Serialize(IMemento value);

        protected abstract IMemento Deserialize(T value);

        public T Serialize(IMemento value, SerializationSettings settings)
        {
            serializationSettings = settings;
            return Serialize(value);
        }

        public IMemento Deserialize(T value, DeserializationSettings settings)
        {
            deserializationSettings = settings;
            return Deserialize(value);
        }
    }

    public interface IObjectSerializer<T, Settings>
    {
        public T Serialize(object value, Settings settings);
    }

    public interface IObjectDeserializer<T, Settings>
    {
        public object Deserialize(Type type, T value, Settings settings);
    }

    public interface IObjectConverter<T, SerializationSettings, DeserializationSettings> : IObjectSerializer<T, SerializationSettings>, IObjectDeserializer<T, DeserializationSettings>
    {
    }

    public abstract class BaseObjectConverter<T, SerializationSettings, DeserializationSettings> : IObjectConverter<T, SerializationSettings, DeserializationSettings>
    {
        protected SerializationSettings serializationSettings;
        protected DeserializationSettings deserializationSettings;

        protected abstract T Serialize(object value);

        protected abstract object Deserialize(Type type, T value);

        public T Serialize(object value, SerializationSettings settings)
        {
            serializationSettings = settings;
            return Serialize(value);
        }

        public object Deserialize(Type type, T value, DeserializationSettings settings)
        {
            deserializationSettings = settings;
            return Deserialize(type, value);
        }
    }

    public abstract class BaseConverter<T, Converter, SerializationSettings, DeserializationSettings> where Converter : IMementoConverter<T, SerializationSettings, DeserializationSettings>, new()
    {
        protected IObjectConverter<IMemento, ToMementoSettings, FromMementoSettings> objectConverter;
        protected IMementoConverter<T, SerializationSettings, DeserializationSettings> converter;

        public BaseConverter()
        {
            objectConverter = new MementoConverter();
            converter = new Converter();
        }

        public T Serialize(object value, ToMementoSettings toMementoSettings, SerializationSettings settings)
        {
            var memento = objectConverter.Serialize(value, toMementoSettings);
            return converter.Serialize(memento, settings);
        }

        public object Deserialize(Type type, T value, FromMementoSettings fromMementoSettings, DeserializationSettings settings)
        {
            var memento = converter.Deserialize(value, settings);
            return objectConverter.Deserialize(type, memento, fromMementoSettings);
        }
    }
}
