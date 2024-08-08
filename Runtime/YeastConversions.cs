using System;
using Yeast.Binary;
using Yeast.Json;
using Yeast.Xml;

namespace Yeast
{
    public static class YeastConversions
    {
        public static string ToJson(this object obj)
        {
            return JSON.Stringify(obj);
        }

        public static bool TryToJson(this object obj, out string json)
        {
            return JSON.TryStringify(obj, out json);
        }

        public static T FromJson<T>(this string json)
        {
            return JSON.Parse<T>(json);
        }

        public static object FromJson(this string json, Type type)
        {
            return JSON.Parse(type, json);
        }

        public static bool TryFromJson<T>(this string json, out T obj)
        {
            return JSON.TryParse(json, out obj);
        }

        public static bool TryFromJson(this string json, Type type, out object obj)
        {
            return JSON.TryParse(type, json, out obj);
        }

        public static byte[] ToBytes(this object obj)
        {
            return BINARY.Serialize(obj);
        }

        public static bool TryToBytes(this object obj, out byte[] bytes)
        {
            return BINARY.TrySerialize(obj, out bytes);
        }

        public static T FromBytes<T>(this byte[] bytes)
        {
            return BINARY.Deserialize<T>(bytes);
        }

        public static object FromBytes(this byte[] bytes, Type type)
        {
            return BINARY.Deserialize(type, bytes);
        }

        public static bool TryFromBytes<T>(this byte[] bytes, out T obj)
        {
            return BINARY.TryDeserialize(bytes, out obj);
        }

        public static bool TryFromBytes(this byte[] bytes, Type type, out object obj)
        {
            return BINARY.TryDeserialize(type, bytes, out obj);
        }

        public static string ToBase64(this object obj)
        {
            return System.Convert.ToBase64String(ToBytes(obj));
        }

        public static bool TryToBase64(this object obj, out string base64)
        {
            if (!TryToBytes(obj, out byte[] bytes))
            {
                base64 = default;
                return false;
            }
            base64 = System.Convert.ToBase64String(bytes);
            return true;
        }

        public static T FromBase64<T>(this string base64)
        {
            return FromBytes<T>(System.Convert.FromBase64String(base64));
        }

        public static object FromBase64(this string base64, Type type)
        {
            return FromBytes(System.Convert.FromBase64String(base64), type);
        }

        public static bool TryFromBase64<T>(this string base64, out T obj)
        {
            byte[] bytes;
            try
            {
                bytes = System.Convert.FromBase64String(base64);
            }
            catch
            {
                obj = default;
                return false;
            }
            return TryFromBytes(bytes, out obj);
        }

        public static bool TryFromBase64(this string base64, Type type, out object obj)
        {
            byte[] bytes;
            try
            {
                bytes = System.Convert.FromBase64String(base64);
            }
            catch
            {
                obj = default;
                return false;
            }
            return TryFromBytes(bytes, type, out obj);
        }

        public static string ToXml(this object obj)
        {
            return XML.Serialize(obj);
        }

        public static bool TryToXml(this object obj, out string xml)
        {
            return XML.TrySerialize(obj, out xml);
        }

        public static T FromXml<T>(this string xml)
        {
            return XML.Deserialize<T>(xml);
        }

        public static object FromXml(this string xml, Type type)
        {
            return XML.Deserialize(type, xml);
        }

        public static bool TryFromXml<T>(this string xml, out T obj)
        {
            return XML.TryDeserialize(xml, out obj);
        }

        public static bool TryFromXml(this string xml, Type type, out object obj)
        {
            return XML.TryDeserialize(type, xml, out obj);
        }
    }
}
