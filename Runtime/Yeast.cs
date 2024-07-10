using System;
using UnityEngine;

public static class Yeast
{
    private static readonly IConverter<string> converter = new JsonConverter();

    public static string Stringify<T>(T value)
    {
        SimpleValue val = SimpleValue.From(value);
        return converter.Stringify(val);
    }

    public static T Parse<T>(string text)
    {
        Debug.Log(text);
        if (!TryParse(text, out T obj, out Exception exception))
        {
            throw exception;
        }
        return obj;
    }

    public static object Parse(Type type, string text)
    {
        if (!TryParse(type, text, out object obj, out Exception exception))
        {
            throw exception;
        }
        return obj;
    }

    public static bool TryParse<T>(string text, out T value)
    {
        return TryParse(text, out value, out _);
    }

    public static bool TryParse<T>(string text, out T value, out Exception exception)
    {
        if (TryParse(typeof(T), text, out object obj, out exception))
        {
            value = (T)obj;
            return true;
        }
        value = default;
        return false;
    }

    public static bool TryParse(Type type, string text, out object value)
    {
        return TryParse(type, text, out value, out _);
    }

    public static bool TryParse(Type type, string text, out object value, out Exception expection)
    {
        SimpleValue val = converter.Parse(type, text);
        return val.TryInto(type, out value, out expection);
    }
}
