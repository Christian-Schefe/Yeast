using System;
using Yeast.Binary;
using Yeast.Json;
using Yeast.Xml;

namespace Yeast
{
    public static class Yeast
    {
        public static string ToJson(object obj)
        {
            return JSON.Stringify(obj);
        }

        public static bool TryToJson(object obj, out string json)
        {
            return JSON.TryStringify(obj, out json);
        }

        public static T FromJson<T>(string json)
        {
            return JSON.Parse<T>(json);
        }

        public static object FromJson(Type type, string json)
        {
            return JSON.Parse(type, json);
        }

        public static bool TryFromJson<T>(string json, out T obj)
        {
            return JSON.TryParse(json, out obj);
        }

        public static bool TryFromJson(Type type, string json, out object obj)
        {
            return JSON.TryParse(type, json, out obj);
        }

        public static byte[] ToBytes(object obj)
        {
            return BINARY.Serialize(obj);
        }

        public static bool TryToBytes(object obj, out byte[] bytes)
        {
            return BINARY.TrySerialize(obj, out bytes);
        }

        public static T FromBytes<T>(byte[] bytes)
        {
            return BINARY.Deserialize<T>(bytes);
        }

        public static object FromBytes(Type type, byte[] bytes)
        {
            return BINARY.Deserialize(type, bytes);
        }

        public static bool TryFromBytes<T>(byte[] bytes, out T obj)
        {
            return BINARY.TryDeserialize(bytes, out obj);
        }

        public static bool TryFromBytes(Type type, byte[] bytes, out object obj)
        {
            return BINARY.TryDeserialize(type, bytes, out obj);
        }

        public static string ToBase64(object obj)
        {
            return Convert.ToBase64String(ToBytes(obj));
        }

        public static bool TryToBase64(object obj, out string base64)
        {
            if (!TryToBytes(obj, out byte[] bytes))
            {
                base64 = default;
                return false;
            }
            base64 = Convert.ToBase64String(bytes);
            return true;
        }

        public static T FromBase64<T>(string base64)
        {
            return FromBytes<T>(Convert.FromBase64String(base64));
        }

        public static object FromBase64(Type type, string base64)
        {
            return FromBytes(type, Convert.FromBase64String(base64));
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

        public static bool TryFromBase64(Type type, string base64, out object obj)
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
            return TryFromBytes(type, bytes, out obj);
        }

        public static string ToXml(object obj)
        {
            return XML.Serialize(obj);
        }

        public static bool TryToXml(object obj, out string xml)
        {
            return XML.TrySerialize(obj, out xml);
        }

        public static T FromXml<T>(string xml)
        {
            return XML.Deserialize<T>(xml);
        }

        public static object FromXml(Type type, string xml)
        {
            return XML.Deserialize(type, xml);
        }

        public static bool TryFromXml<T>(string xml, out T obj)
        {
            return XML.TryDeserialize(xml, out obj);
        }

        public static bool TryFromXml(Type type, string xml, out object obj)
        {
            return XML.TryDeserialize(type, xml, out obj);
        }
    }
}
