using System;

namespace Yeast
{
    public interface IIntoConverter<X, Y, S, E> where E : Exception
    {
        public bool TryInto(X value, out Y result, S settings, out E exception);
    }

    public interface IFromConverter<X, Y, S, E> where E : Exception
    {
        public bool TryFrom(X value, out Y result, S settings, out E exception);
    }
}
