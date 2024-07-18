using System;
using Yeast.Memento;

namespace Yeast
{
    public interface IMementoSerializer<out T>
    {
        public T Serialize(IMemento value);
    }

    public interface IMementoDeserializer<T>
    {
        public IMemento Deserialize(T value);
    }

    public interface IMementoConverter<T> : IMementoSerializer<T>, IMementoDeserializer<T>
    {
    }
}
