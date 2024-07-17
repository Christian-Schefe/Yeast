using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Windows;
using Yeast.Json;

namespace Yeast.Utils
{
    public static class StringUtils
    {

        public static string EscapeString(string input)
        {
            StringBuilder literal = new(input.Length);
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default:
                        // ASCII printable character
                        if (c >= 0x20 && c <= 0x7e)
                        {
                            literal.Append(c);
                            // As UTF16 escaped character
                        }
                        else
                        {
                            literal.Append(@"\u");
                            literal.Append(((int)c).ToString("x4"));
                        }
                        break;
                }
            }
            return literal.ToString();
        }

        public static string UnescapeString(string literal)
        {
            StringBuilder result = new();

            int i = 0;
            while (i < literal.Length)
            {
                char c = literal[i];
                if (c == '\\')
                {
                    // Handle escape sequences
                    if (i + 1 >= literal.Length) throw new JsonConversionException($"Unexpected end of string {literal}", i);
                    char nextChar = literal[i + 1];
                    switch (nextChar)
                    {
                        case '\\': result.Append("\\"); break;
                        case 'n': result.Append("\n"); break;
                        case 't': result.Append("\t"); break;
                        case 'r': result.Append("\r"); break;
                        case 'b': result.Append("\b"); break;
                        case 'f': result.Append("\f"); break;
                        case 'a': result.Append("\a"); break;
                        case 'v': result.Append("\v"); break;
                        case '0': result.Append("\0"); break;
                        case '\"': result.Append("\""); break;
                        case 'u':
                            // Parse Unicode escape sequence
                            string unicodeValue = literal.Substring(i + 2, 4);
                            result.Append((char)int.Parse(unicodeValue, NumberStyles.HexNumber));
                            i += 4;
                            break;
                        default:
                            // Unrecognized escape sequence, treat as a literal character
                            result.Append(c);
                            break;
                    }
                    i += 2;
                }
                else
                {
                    // Regular character
                    result.Append(c);
                    i++;
                }
            }

            return result.ToString();
        }

        public static string HTMLEscape(string input)
        {
            StringBuilder literal = new(input.Length);
            foreach (var c in input)
            {
                switch (c)
                {
                    case '<': literal.Append("&lt;"); break;
                    case '>': literal.Append("&gt;"); break;
                    case '&': literal.Append("&amp;"); break;
                    default:
                        literal.Append(c);
                        break;
                }
            }
            return literal.ToString();
        }

        public static string HTMLUnescape(string literal)
        {
            StringBuilder result = new();

            int i = 0;
            while (i < literal.Length)
            {
                char c = literal[i];
                if (c == '&')
                {
                    string strToEnd = literal[i..];
                    if (strToEnd.StartsWith("&lt;"))
                    {
                        result.Append("<");
                        i += 4;
                    }
                    else if (strToEnd.StartsWith("&gt;"))
                    {
                        result.Append(">");
                        i += 4;
                    }
                    else if (strToEnd.StartsWith("&amp;"))
                    {
                        result.Append("&");
                        i += 5;
                    }
                    else
                    {
                        throw new JsonConversionException($"Unknown HTML escape sequence {literal}", i);
                    }
                }
                else
                {
                    // Regular character
                    result.Append(c);
                    i++;
                }
            }

            return result.ToString();
        }

        public static string LongToString(long val)
        {
            return val.ToString(CultureInfo.InvariantCulture);
        }

        public static long StringToLong(string val)
        {
            return long.Parse(val, CultureInfo.InvariantCulture);
        }

        public static string DoubleToString(double val)
        {
            var str = val.ToString("R", CultureInfo.InvariantCulture);
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c != '-' && !char.IsDigit(c)) return str;
            }
            return str + ".0";
        }

        public static double StringToDouble(string val)
        {
            return double.Parse(val, CultureInfo.InvariantCulture);
        }
    }
}
