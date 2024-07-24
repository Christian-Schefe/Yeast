using System;
using System.Collections.Generic;
using System.Reflection;

namespace Yeast.Utils
{
    public class CollectionWrapper
    {
        private static readonly Dictionary<Type, Action<object, object>> addDelegates = new();

        private readonly Action<object, object> addMethod;
        private readonly object collection;

        public CollectionWrapper(Type elementType, object collection)
        {
            this.collection = collection;
            if (addDelegates.TryGetValue(elementType, out addMethod))
            {
                return;
            }

            try
            {
                var delegateCreator = typeof(CollectionWrapper).GetMethod(nameof(GetAddDelegate), BindingFlags.NonPublic | BindingFlags.Static);
                var genericDelegateCreator = delegateCreator.MakeGenericMethod(elementType);
                addMethod = (Action<object, object>)genericDelegateCreator.Invoke(null, new object[0]);
                addDelegates.Add(elementType, addMethod);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to cache delegate: " + e.Message);
            }
        }

        private static Action<object, object> GetAddDelegate<T>()
        {
            return (colObj, itemObj) =>
            {
                ICollection<T> col = (ICollection<T>)colObj;
                col.Add((T)itemObj);
            };
        }

        public void Add(object value)
        {
            addMethod(collection, value);
        }

        public object GetCollection()
        {
            return collection;
        }
    }
}
