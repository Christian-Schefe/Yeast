using System;
using Yeast.Binary;
using Yeast.Json;

namespace Yeast
{
    public static class Yeast
    {
        public static string ToJson(object obj, JsonStringifyMode mode = JsonStringifyMode.Compact)
        {
            return JSON.Stringify(obj, mode);
        }

        public static T FromJson<T>(string json, JsonParseMode mode = JsonParseMode.Exact)
        {
            return JSON.Parse<T>(json, mode);
        }

        public static bool TryFromJson<T>(string json, out T obj, JsonParseMode mode = JsonParseMode.Exact)
        {
            return JSON.TryParse(json, out obj, mode);
        }

        public static byte[] ToBytes(object obj)
        {
            return BINARY.Serialize(obj);
        }

        public static T FromBytes<T>(byte[] bytes)
        {
            return BINARY.Deserialize<T>(bytes);
        }

        public static bool TryFromBytes<T>(byte[] bytes, out T obj)
        {
            return BINARY.TryDeserialize(bytes, out obj);
        }

        public static string ToBase64(object obj)
        {
            return Convert.ToBase64String(ToBytes(obj));
        }

        public static T FromBase64<T>(string base64)
        {
            return FromBytes<T>(Convert.FromBase64String(base64));
        }

        public static bool TryFromBase64<T>(string base64, out T obj)
        {
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64);
            }
            catch
            {
                obj = default;
                return false;
            }
            return TryFromBytes(bytes, out obj);
        }
    }
}
