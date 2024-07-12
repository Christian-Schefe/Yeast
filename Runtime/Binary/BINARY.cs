using System;
using Yeast.Ion;
using Yeast.Utils;

namespace Yeast.Binary
{
    public sealed class BINARY : IFromConverter<(Type, byte[]), object, (FromBinarySettings, FromIonSettings), Exception>, IIntoConverter<object, byte[], (ToBinarySettings, ToIonSettings), Exception>
    {
        private static readonly BINARY instance = new();

        private readonly IonConverter ionConverter = new();
        private readonly BinaryConverter binaryConverter = new();

        private BINARY() { }

        private static (ToBinarySettings, ToIonSettings) CreateSettings(BinaryStringifyMode mode)
        {
            return mode switch
            {
                BinaryStringifyMode.Compact => (new ToBinarySettings() , new ToIonSettings() { maxDepth = 100 }),
                BinaryStringifyMode.Pretty => (new ToBinarySettings(), new ToIonSettings { maxDepth = 100 }),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        private static (FromBinarySettings, FromIonSettings) CreateSettings(BinaryParseMode mode)
        {
            return mode switch
            {
                BinaryParseMode.Exact => (new FromBinarySettings(), new FromIonSettings { ignoreExtraFields = false, useDefaultSetting = FromIonSettings.UseDefaultSetting.Never }),
                BinaryParseMode.Loose => (new FromBinarySettings(), new FromIonSettings { ignoreExtraFields = true, useDefaultSetting = FromIonSettings.UseDefaultSetting.ForMissingOrMismatchedFields }),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        public static byte[] Stringify<T>(T value, BinaryStringifyMode mode = BinaryStringifyMode.Compact)
        {
            var settings = CreateSettings(mode);
            return instance.TryInto(value, out byte[] result, settings, out var exception) ? result : throw exception;
        }

        public static bool TryStringify<T>(T value, out byte[] result, BinaryStringifyMode mode = BinaryStringifyMode.Compact)
        {
            var settings = CreateSettings(mode);
            return instance.TryInto(value, out result, settings, out _);
        }

        public static T Parse<T>(byte[] text, BinaryParseMode mode = BinaryParseMode.Exact)
        {
            var settings = CreateSettings(mode);
            return instance.TryFrom((typeof(T), text), out object result, settings, out var exception) ? (T)result : throw exception;
        }

        public static bool TryParse<T>(byte[] text, out T result, BinaryParseMode mode = BinaryParseMode.Exact)
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

        public bool TryFrom((Type, byte[]) value, out object result, (FromBinarySettings, FromIonSettings) settings, out Exception exception)
        {
            if (!binaryConverter.TryFrom(value.Item2, out IIonValue ionValue, settings.Item1, out var e))
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

        public bool TryInto(object value, out byte[] result, (ToBinarySettings, ToIonSettings) settings, out Exception exception)
        {
            if (!ionConverter.TryInto(value, out IIonValue ionValue, settings.Item2, out var e))
            {
                result = null;
                exception = e;
                return false;
            }
            if (!binaryConverter.TryInto(ionValue, out result, settings.Item1, out var ex))
            {
                exception = ex;
                return false;
            }
            exception = null;
            return true;
        }
    }

    public enum BinaryStringifyMode
    {
        Compact,
        Pretty
    }
    public enum BinaryParseMode
    {
        Exact,
        Loose
    }
}
