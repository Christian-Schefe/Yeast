using System;
using System.Collections.Generic;
using Yeast.Memento;

namespace Yeast.Json
{
    /// <summary>
    /// Converts objects to and from JSON format.
    /// </summary>
    public sealed class JSON
    {
        private static readonly JSON instance = new();

        private readonly MementoConverter mementoConverter;
        private readonly ToJsonMementoVisitor visitor;
        private readonly JsonTypeWrapperVisitor typeVisitor;

        private JSON()
        {
            mementoConverter = new();
            visitor = new();
            typeVisitor = new();
        }

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        public static string Stringify(object value)
        {
            var memento = instance.mementoConverter.Serialize(value);
            memento.Accept(instance.visitor);
            return instance.visitor.GetResult().ToString();
        }

        /// <summary>
        /// Tries to convert an object to a JSON string.
        /// </summary>
        public static bool TryStringify(object value, out string result)
        {
            try
            {
                result = Stringify(value);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Converts a JSON string to an object.
        /// </summary>
        public static T Parse<T>(string text)
        {
            return (T)Parse(typeof(T), text);
        }

        /// <summary>
        /// Tries to convert a JSON string to an object.
        /// </summary>
        public static bool TryParse<T>(string text, out T result)
        {
            try
            {
                result = Parse<T>(text);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Converts a JSON string to an object.
        /// </summary>
        public static object Parse(Type type, string text)
        {
            var jsonValue = JsonValue.FromString(text);
            var memento = instance.typeVisitor.Convert(jsonValue, TypeWrapper.FromType(type));
            UnityEngine.Debug.Log(memento);
            return instance.mementoConverter.Deserialize(type, memento);
        }

        /// <summary>
        /// Tries to convert a JSON string to an object.
        /// </summary>
        public static bool TryParse(Type type, string text, out object result)
        {
            try
            {
                result = Parse(type, text);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
