using System;
using Yeast.Memento;

namespace Yeast.Xml
{
    public class XML
    {
        private static readonly XML instance = new();

        private readonly MementoConverter mementoConverter;
        private readonly XmlConverter xmlConverter;

        private XML()
        {
            mementoConverter = new();
            xmlConverter = new();
        }


        /// <summary>
        /// Converts an object to XML.
        /// </summary>
        public static string Serialize(object value)
        {
            var memento = instance.mementoConverter.Serialize(value);
            return instance.xmlConverter.Serialize(memento);
        }

        /// <summary>
        /// Tries to convert an object to XML.
        /// </summary>
        public static bool TrySerialize(object value, out string result)
        {
            try
            {
                result = Serialize(value);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Converts XML to an object.
        /// </summary>
        public static T Deserialize<T>(string text)
        {
            return (T)Deserialize(typeof(T), text);
        }

        /// <summary>
        /// Tries to convert XML to an object.
        /// </summary>
        public static bool TryDeserialize<T>(string text, out T result)
        {
            try
            {
                result = Deserialize<T>(text);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Converts XML to an object.
        /// </summary>
        public static object Deserialize(Type type, string bytes)
        {
            var memento = instance.xmlConverter.Deserialize(bytes);
            return instance.mementoConverter.Deserialize(type, memento);
        }

        /// <summary>
        /// Tries to convert XML to an object.
        /// </summary>
        public static bool TryDeserialize(Type type, string bytes, out object result)
        {
            try
            {
                result = Deserialize(type, bytes);
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
