using System;
using Yeast.Json;
using Yeast.Memento;

namespace Yeast.Xml
{
    public class XML
    {
        private static readonly XML instance = new();

        private readonly MementoConverter mementoConverter;

        private XML()
        {
            mementoConverter = new();
        }


        /// <summary>
        /// Converts an object to XML.
        /// </summary>
        public static string Serialize(object value)
        {
            var memento = instance.mementoConverter.Serialize(value);
            var mementoVisitor = new ToXmlMementoVisitor();
            memento.Accept(mementoVisitor);
            return mementoVisitor.GetResult().ToString();
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
        public static object Deserialize(Type type, string text)
        {
            var xmlDocument = XmlDocument.FromString(text);
            var translator = new XmlToMementoTranslator();
            var memento = translator.Convert(xmlDocument.root, type);
            return instance.mementoConverter.Deserialize(type, memento);
        }

        /// <summary>
        /// Tries to convert XML to an object.
        /// </summary>
        public static bool TryDeserialize(Type type, string text, out object result)
        {
            try
            {
                result = Deserialize(type, text);
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
