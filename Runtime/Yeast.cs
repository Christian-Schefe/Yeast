using System;
using System.Collections.Generic;

public static class Yeast
{
    private static readonly IConverter<string> converter = new JsonConverter();

    private static readonly Stack<ConversionSettings> settingsStack = new();

    public static void PushSettings(ConversionSettings settings)
    {
        settingsStack.Push(settings);
    }

    public static void PopSettings()
    {
        settingsStack.Pop();
    }

    public static ConversionSettings CurrentConversionSettings => settingsStack.Count > 0 ? settingsStack.Peek() : ConversionSettings.Default;

    public static string Stringify<T>(T value)
    {
        SimpleValue val = SimpleValue.From(value);
        return converter.Stringify(val);
    }


    public static T Parse<T>(string text)
    {
        return Parse<T>(text, CurrentConversionSettings);
    }

    public static T Parse<T>(string text, ConversionSettings settings)
    {
        if (!TryParse(text, settings, out T obj, out Exception exception))
        {
            throw exception;
        }
        return obj;
    }


    public static object Parse(Type type, string text)
    {
        return Parse(type, text, CurrentConversionSettings);
    }

    public static object Parse(Type type, string text, ConversionSettings settings)
    {
        if (!TryParse(type, text, settings, out object obj, out Exception exception))
        {
            throw exception;
        }
        return obj;
    }


    public static bool TryParse<T>(string text, out T value)
    {
        return TryParse(text, CurrentConversionSettings, out value, out _);
    }

    public static bool TryParse<T>(string text, out T value, out Exception exception)
    {
        return TryParse(text, CurrentConversionSettings, out value, out exception);
    }

    public static bool TryParse<T>(string text, ConversionSettings settings, out T value)
    {
        return TryParse(text, settings, out value, out _);
    }

    public static bool TryParse<T>(string text, ConversionSettings settings, out T value, out Exception exception)
    {
        if (TryParse(typeof(T), text, settings, out object obj, out exception))
        {
            value = (T)obj;
            return true;
        }
        value = default;
        return false;
    }


    public static bool TryParse(Type type, string text, out object value)
    {
        return TryParse(type, text, CurrentConversionSettings, out value, out _);
    }

    public static bool TryParse(Type type, string text, ConversionSettings settings, out object value)
    {
        return TryParse(type, text, settings, out value, out _);
    }

    public static bool TryParse(Type type, string text, out object value, out Exception exception)
    {
        return TryParse(type, text, CurrentConversionSettings, out value, out exception);
    }

    public static bool TryParse(Type type, string text, ConversionSettings settings, out object value, out Exception expection)
    {
        SimpleValue val = converter.Parse(type, text);
        return val.TryInto(settings, type, out value, out expection);
    }
}
