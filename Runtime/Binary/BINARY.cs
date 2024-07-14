using System;
using Yeast.Ion;

namespace Yeast.Binary
{
    public sealed class BINARY : BaseConverter<byte[], BinaryConverter, BinarySerializationSettings, BinaryDeserializationSettings>
    {
        private static readonly BINARY instance = new();

        private BINARY() : base() { }

        private static (BinarySerializationSettings, ToIonSettings) CreateSerializationSettings()
        {
            return (new BinarySerializationSettings(), new ToIonSettings() { maxDepth = 100 });
        }

        private static (BinaryDeserializationSettings, FromIonSettings) CreateDeserializationSettings()
        {
            return (new BinaryDeserializationSettings(), new FromIonSettings { ignoreExtraFields = false, useDefaultSetting = FromIonSettings.UseDefaultSetting.Never });
        }

        public static byte[] Serialize<T>(T value)
        {
            var settings = CreateSerializationSettings();
            return instance.Serialize(value, settings.Item2, settings.Item1);
        }

        public static bool TrySerialize<T>(T value, out byte[] result)
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

        public static T Deserialize<T>(byte[] text)
        {
            var settings = CreateDeserializationSettings();
            return (T)instance.Deserialize(typeof(T), text, settings.Item2, settings.Item1);
        }

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
    }
}
