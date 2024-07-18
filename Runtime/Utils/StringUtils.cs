using System.Globalization;
using System.Text;

namespace Yeast.Utils
{
    public static class StringUtils
    {
        public static string EscapeJsonString(string input)
        {
            StringBuilder literal = new(input.Length);
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '/': literal.Append(@"\/"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    default:
                        // ASCII printable character
                        if (c >= 0x20 && c <= 0x7e)
                        {
                            literal.Append(c);
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

        public static string UnescapeJsonString(string literal)
        {
            StringBuilder result = new();

            int i = 0;
            while (i < literal.Length)
            {
                char c = literal[i];
                if (c == '\\')
                {
                    if (i + 1 >= literal.Length) throw new System.IndexOutOfRangeException($"Unexpected end of string {literal}");
                    char nextChar = literal[i + 1];
                    switch (nextChar)
                    {
                        case '\"': result.Append("\""); break;
                        case '\\': result.Append("\\"); break;
                        case '/': result.Append("/"); break;
                        case 'b': result.Append("\b"); break;
                        case 'f': result.Append("\f"); break;
                        case 'n': result.Append("\n"); break;
                        case 'r': result.Append("\r"); break;
                        case 't': result.Append("\t"); break;
                        case 'u':
                            // Parse Unicode escape sequence
                            string unicodeValue = literal.Substring(i + 2, 4);
                            result.Append((char)int.Parse(unicodeValue, NumberStyles.HexNumber));
                            i += 4;
                            break;
                        default:
                            throw new System.InvalidOperationException($"Unknown escape sequence '\\{nextChar}'");
                    }
                    i += 2;
                }
                else
                {
                    result.Append(c);
                    i++;
                }
            }

            return result.ToString();
        }

        public static string EscapeXMLString(string input)
        {
            StringBuilder literal = new(input.Length);
            foreach (var c in input)
            {
                switch (c)
                {
                    case '<': literal.Append("&lt;"); break;
                    case '>': literal.Append("&gt;"); break;
                    case '&': literal.Append("&amp;"); break;
                    case '\"': literal.Append("&quot;"); break;
                    case '\'': literal.Append("&apos;"); break;
                    default:
                        literal.Append(c);
                        break;
                }
            }
            return literal.ToString();
        }

        public static string UnescapeXMLString(string literal)
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
                    else if (strToEnd.StartsWith("&quot;"))
                    {
                        result.Append("\"");
                        i += 6;
                    }
                    else if (strToEnd.StartsWith("&apos;"))
                    {
                        result.Append("'");
                        i += 6;
                    }
                    else
                    {
                        throw new System.InvalidOperationException($"Unknown HTML escape sequence {literal}");
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
            if (double.IsNaN(val)) return "NaN";
            if (double.IsPositiveInfinity(val)) return "Infinity";
            if (double.IsNegativeInfinity(val)) return "-Infinity";
            return val.ToString("R", CultureInfo.InvariantCulture);
        }

        public static string DoubleToStringWithZero(double val)
        {
            var str = DoubleToString(val);
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c != '-' && !char.IsDigit(c)) return str;
            }
            return str + ".0";
        }

        public static double StringToDouble(string val)
        {
            if (val == "NaN") return double.NaN;
            if (val == "Infinity") return double.PositiveInfinity;
            if (val == "-Infinity") return double.NegativeInfinity;
            return double.Parse(val, CultureInfo.InvariantCulture);
        }
    }
}
