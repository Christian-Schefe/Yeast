using System;
using System.Collections.Generic;
using System.Reflection;

namespace Yeast.Utils
{
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
}
