using System;
using Yeast.Memento;

namespace Yeast.Xml
{
    public class XML : BaseConverter<string, XmlConverter, XmlSerializationSettings, XmlDeserializationSettings>
    {
        private static readonly XML instance = new();

        private XML() : base() { }

        private static (XmlSerializationSettings, ToMementoSettings) CreateSerializationSettings()
        {
            return (new XmlSerializationSettings(), new ToMementoSettings() { maxDepth = 100 });
        }

        private static (XmlDeserializationSettings, FromMementoSettings) CreateDeserializationSettings()
        {
            return (new XmlDeserializationSettings(), new FromMementoSettings { ignoreExtraFields = false, useDefaultSetting = FromMementoSettings.UseDefaultSetting.Never });
        }

        /// <summary>
        /// Converts an object to XML.
        /// </summary>
        public static string Serialize(object value)
        {
            var settings = CreateSerializationSettings();
            return instance.Serialize(value, settings.Item2, settings.Item1);
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
            var settings = CreateDeserializationSettings();
            return instance.Deserialize(type, bytes, settings.Item2, settings.Item1);
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
