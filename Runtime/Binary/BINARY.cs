using System;
using Yeast.Memento;
using Yeast.Json;

namespace Yeast.Binary
{
    /// <summary>
    /// Converts objects to and from binary format.
    /// </summary>
    public sealed class BINARY : BaseConverter<byte[], BinaryConverter, BinarySerializationSettings, BinaryDeserializationSettings>
    {
        private static readonly BINARY instance = new();

        private BINARY() : base() { }

        private static (BinarySerializationSettings, ToMementoSettings) CreateSerializationSettings()
        {
            return (new BinarySerializationSettings(), new ToMementoSettings() { maxDepth = 100 });
        }

        private static (BinaryDeserializationSettings, FromMementoSettings) CreateDeserializationSettings()
        {
            return (new BinaryDeserializationSettings(), new FromMementoSettings { ignoreExtraFields = false, useDefaultSetting = FromMementoSettings.UseDefaultSetting.Never });
        }

        /// <summary>
        /// Converts an object to a byte array.
        /// </summary>
        public static byte[] Serialize(object value)
        {
            var settings = CreateSerializationSettings();
            return instance.Serialize(value, settings.Item2, settings.Item1);
        }

        /// <summary>
        /// Tries to convert an object to a byte array.
        /// </summary>
        public static bool TrySerialize(object value, out byte[] result)
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
        /// Converts a byte array to an object.
        /// </summary>
        public static T Deserialize<T>(byte[] text)
        {
            return (T)Deserialize(typeof(T), text);
        }

        /// <summary>
        /// Tries to convert a byte array to an object.
        /// </summary>
        public static bool TryDeserialize<T>(byte[] text, out T result)
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
        /// Converts a byte array to an object.
        /// </summary>
        public static object Deserialize(Type type, byte[] bytes)
        {
            var settings = CreateDeserializationSettings();
            return instance.Deserialize(type, bytes, settings.Item2, settings.Item1);
        }

        /// <summary>
        /// Tries to convert a byte array to an object.
        /// </summary>
        public static bool TryDeserialize(Type type, byte[] bytes, out object result)
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
