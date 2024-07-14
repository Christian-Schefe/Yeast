using System;
using Yeast.Ion;

namespace Yeast.Json
{
    /// <summary>
    /// Converts objects to and from JSON format.
    /// </summary>
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

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        public static string Stringify(object value, JsonStringifyMode mode = JsonStringifyMode.Compact)
        {
            var settings = CreateSettings(mode);
            return instance.Serialize(value, settings.Item2, settings.Item1);
        }

        /// <summary>
        /// Converts a JSON string to an object.
        /// </summary>
        public static T Parse<T>(string text, JsonParseMode mode = JsonParseMode.Exact)
        {
            return (T)Parse(typeof(T), text, mode);
        }

        /// <summary>
        /// Tries to convert a JSON string to an object.
        /// </summary>
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

        /// <summary>
        /// Converts a JSON string to an object.
        /// </summary>
        public static object Parse(Type type, string text, JsonParseMode mode = JsonParseMode.Exact)
        {
            var settings = CreateSettings(mode);
            return instance.Deserialize(type, text, settings.Item2, settings.Item1);
        }

        /// <summary>
        /// Tries to convert a JSON string to an object.
        /// </summary>
        public static bool TryParse(Type type, string text, out object result, JsonParseMode mode = JsonParseMode.Exact)
        {
            try
            {
                result = Parse(type, text, mode);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }

    /// <summary>
    /// Modes for converting objects to JSON strings.
    /// </summary>
    public enum JsonStringifyMode
    {
        Compact,
        Pretty
    }

    /// <summary>
    /// Modes for converting JSON strings to objects.
    /// </summary>
    public enum JsonParseMode
    {
        Exact,
        Loose
    }
}
