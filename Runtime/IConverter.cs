using System;
using Yeast.Ion;

namespace Yeast
{
    public interface IIonSerializer<out T, Settings>
    {
        public T Serialize(IIonValue value, Settings settings);
    }

    public interface IIonDeserializer<T, Settings>
    {
        public IIonValue Deserialize(T value, Settings settings);
    }

    public interface IIonConverter<T, SerializationSettings, DeserializationSettings> : IIonSerializer<T, SerializationSettings>, IIonDeserializer<T, DeserializationSettings>
    {
    }

    public abstract class BaseIonConverter<T, SerializationSettings, DeserializationSettings> : IIonConverter<T, SerializationSettings, DeserializationSettings>
    {
        protected SerializationSettings serializationSettings;
        protected DeserializationSettings deserializationSettings;

        protected abstract T Serialize(IIonValue value);

        protected abstract IIonValue Deserialize(T value);

        public T Serialize(IIonValue value, SerializationSettings settings)
        {
            serializationSettings = settings;
            return Serialize(value);
        }

        public IIonValue Deserialize(T value, DeserializationSettings settings)
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

    public abstract class BaseConverter<T, Converter, SerializationSettings, DeserializationSettings> where Converter : IIonConverter<T, SerializationSettings, DeserializationSettings>, new()
    {
        protected IObjectConverter<IIonValue, ToIonSettings, FromIonSettings> objectConverter;
        protected IIonConverter<T, SerializationSettings, DeserializationSettings> converter;

        public BaseConverter()
        {
            objectConverter = new IonConverter();
            converter = new Converter();
        }

        public T Serialize(object value, ToIonSettings toIonSettings, SerializationSettings settings)
        {
            var ionValue = objectConverter.Serialize(value, toIonSettings);
            return converter.Serialize(ionValue, settings);
        }

        public object Deserialize(Type type, T value, FromIonSettings fromIonSettings, DeserializationSettings settings)
        {
            var ionValue = converter.Deserialize(value, settings);
            return objectConverter.Deserialize(type, ionValue, fromIonSettings);
        }
    }
}
