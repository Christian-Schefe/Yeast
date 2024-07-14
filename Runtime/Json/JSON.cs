using System;
using Yeast.Ion;

namespace Yeast.Json
{
    public sealed class JSON : BaseConverter<string, JsonConverter, JsonSerializationSettings, JsonDeserializationSettings>
    {
        private static readonly JSON instance = new();

        private JSON() : base() { }

        private static (JsonSerializationSettings, ToIonSettings) CreateSettings(JsonStringifyMode mode)
        {
            return mode switch
            {
                JsonStringifyMode.Compact => (new JsonSerializationSettings() { prettyPrint = false, indentSize = 0 }, new ToIonSettings() { maxDepth = 100 }),
                JsonStringifyMode.Pretty => (new JsonSerializationSettings() { prettyPrint = true, indentSize = 2 }, new ToIonSettings() { maxDepth = 100 }),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        private static (JsonDeserializationSettings, FromIonSettings) CreateSettings(JsonParseMode mode)
        {
            return mode switch
            {
                JsonParseMode.Exact => (new JsonDeserializationSettings(), new FromIonSettings() { ignoreExtraFields = false, useDefaultSetting = FromIonSettings.UseDefaultSetting.Never }),
                JsonParseMode.Loose => (new JsonDeserializationSettings(), new FromIonSettings() { ignoreExtraFields = true, useDefaultSetting = FromIonSettings.UseDefaultSetting.ForMissingOrMismatchedFields }),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        public static string Stringify<T>(T value, JsonStringifyMode mode = JsonStringifyMode.Compact)
        {
            var settings = CreateSettings(mode);
            return instance.Serialize(value, settings.Item2, settings.Item1);
        }

        public static bool TryStringify<T>(T value, out string result, JsonStringifyMode mode = JsonStringifyMode.Compact)
        {
            try
            {
                result = Stringify(value, mode);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static T Parse<T>(string text, JsonParseMode mode = JsonParseMode.Exact)
        {
            var settings = CreateSettings(mode);
            return (T)instance.Deserialize(typeof(T), text, settings.Item2, settings.Item1);
        }

        public static bool TryParse<T>(string text, out T result, JsonParseMode mode = JsonParseMode.Exact)
        {
            try
            {
                result = Parse<T>(text, mode);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }

    public enum JsonStringifyMode
    {
        Compact,
        Pretty
    }
    public enum JsonParseMode
    {
        Exact,
        Loose
    }
}
