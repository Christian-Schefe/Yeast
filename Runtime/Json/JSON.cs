using System;
using Yeast.Ion;
using Yeast.Utils;

namespace Yeast.Json
{
    public sealed class JSON : IFromConverter<(Type, string), object, (FromJsonSettings, FromIonSettings), Exception>, IIntoConverter<object, string, (ToJsonSettings, ToIonSettings), Exception>
    {
        private static readonly JSON instance = new();

        private readonly IonConverter ionConverter = new();
        private readonly JsonConverter jsonConverter = new();

        private JSON() { }

        private static (ToJsonSettings, ToIonSettings) CreateSettings(JsonStringifyMode mode)
        {
            return mode switch
            {
                JsonStringifyMode.Compact => (new ToJsonSettings() { prettyPrint = false, indentSize = 0 }, new ToIonSettings() { maxDepth = 100 }),
                JsonStringifyMode.Pretty => (new ToJsonSettings() { prettyPrint = true, indentSize = 2 }, new ToIonSettings() { maxDepth = 100 }),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        private static (FromJsonSettings, FromIonSettings) CreateSettings(JsonParseMode mode)
        {
            return mode switch
            {
                JsonParseMode.Exact => (new FromJsonSettings(), new FromIonSettings() { ignoreExtraFields = false, useDefaultSetting = FromIonSettings.UseDefaultSetting.Never }),
                JsonParseMode.Loose => (new FromJsonSettings(), new FromIonSettings() { ignoreExtraFields = true, useDefaultSetting = FromIonSettings.UseDefaultSetting.ForMissingOrMismatchedFields }),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        public static string Stringify<T>(T value, JsonStringifyMode mode = JsonStringifyMode.Compact)
        {
            var settings = CreateSettings(mode);
            return instance.TryInto(value, out string result, settings, out var exception) ? result : throw exception;
        }

        public static bool TryStringify<T>(T value, out string result, JsonStringifyMode mode = JsonStringifyMode.Compact)
        {
            var settings = CreateSettings(mode);
            return instance.TryInto(value, out result, settings, out _);
        }

        public static T Parse<T>(string text, JsonParseMode mode = JsonParseMode.Exact)
        {
            var settings = CreateSettings(mode);
            return instance.TryFrom((typeof(T), text), out object result, settings, out var exception) ? (T)result : throw exception;
        }

        public static bool TryParse<T>(string text, out T result, JsonParseMode mode = JsonParseMode.Exact)
        {
            var settings = CreateSettings(mode);

            if (instance.TryFrom((typeof(T), text), out object value, settings, out _))
            {
                result = (T)value;
                return true;
            }
            result = default;
            return false;
        }

        public bool TryFrom((Type, string) value, out object result, (FromJsonSettings, FromIonSettings) settings, out Exception exception)
        {
            if (!jsonConverter.TryFrom(value.Item2, out IIonValue ionValue, settings.Item1, out var e))
            {
                result = TypeUtils.DefaultValue(value.Item1);
                exception = e;
                return false;
            }
            if (!ionConverter.TryFrom((ionValue, value.Item1), out result, settings.Item2, out var ex))
            {
                exception = ex;
                return false;
            }
            exception = null;
            return true;
        }

        public bool TryInto(object value, out string result, (ToJsonSettings, ToIonSettings) settings, out Exception exception)
        {
            if (!ionConverter.TryInto(value, out IIonValue ionValue, settings.Item2, out var e))
            {
                result = null;
                exception = e;
                return false;
            }
            if (!jsonConverter.TryInto(ionValue, out result, settings.Item1, out var ex))
            {
                exception = ex;
                return false;
            }
            exception = null;
            return true;
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
