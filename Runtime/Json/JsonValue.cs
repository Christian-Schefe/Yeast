using System.Collections.Generic;
using System.Text;
using Yeast.Utils;

namespace Yeast.Json
{
    public abstract class JsonValue
    {
        public abstract override string ToString();
        public abstract bool ValueEquals(JsonValue obj);
        public abstract void Accept(IJsonVisitor visitor);

        public abstract object GetValue();

        public static JsonValue FromString(string json)
        {
            var parser = new JsonParser(json);
            return parser.ParseValue();
        }

        public int AsInt()
        {
            if (this is JsonNumber num) return (int)num.value;
            else throw new System.InvalidCastException($"Cannot convert {GetType().Name} to int");
        }

        public long AsLong()
        {
            if (this is JsonNumber num) return (long)num.value;
            else throw new System.InvalidCastException($"Cannot convert {GetType().Name} to long");
        }

        public float AsFloat()
        {
            if (this is JsonNumber num) return (float)num.value;
            else throw new System.InvalidCastException($"Cannot convert {GetType().Name} to float");
        }

        public double AsDouble()
        {
            if (this is JsonNumber num) return num.value;
            else throw new System.InvalidCastException($"Cannot convert {GetType().Name} to double");
        }

        public string AsString()
        {
            if (this is JsonString str) return str.value;
            else throw new System.InvalidCastException($"Cannot convert {GetType().Name} to string");
        }

        public bool AsBool()
        {
            if (this is JsonBoolean boolean) return boolean.value;
            else throw new System.InvalidCastException($"Cannot convert {GetType().Name} to bool");
        }

        public List<JsonValue> AsArray()
        {
            if (this is JsonArray arr) return arr.value;
            else throw new System.InvalidCastException($"Cannot convert {GetType().Name} to array");
        }

        public Dictionary<string, JsonValue> AsObject()
        {
            if (this is JsonObject obj) return obj.value;
            else throw new System.InvalidCastException($"Cannot convert {GetType().Name} to object");
        }

        public JsonValue Get(string path)
        {
            var parts = path.Split('.');
            JsonValue current = this;
            foreach (var part in parts)
            {
                current = current[part];
            }
            return current;
        }

        public JsonValue this[string key] => AsObject()[key];

        public JsonValue this[int index] => AsArray()[index];
    }

    public class JsonString : JsonValue
    {
        public string value;
        public override object GetValue() => value;

        public JsonString(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('"');
            sb.Append(StringUtils.EscapeJsonString(value));
            sb.Append('"');
            return sb.ToString();
        }

        public override bool ValueEquals(JsonValue obj)
        {
            return obj is JsonString str && str.value.Equals(value);
        }

        public override void Accept(IJsonVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class JsonNumber : JsonValue
    {
        public double value;
        public override object GetValue() => value;

        public JsonNumber(double value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return StringUtils.DoubleToString(value);
        }

        public override bool ValueEquals(JsonValue obj)
        {
            return obj is JsonNumber num && (num.value == value || double.IsNaN(value) && double.IsNaN(num.value));
        }

        public override void Accept(IJsonVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class JsonBoolean : JsonValue
    {
        public bool value;
        public override object GetValue() => value;

        public JsonBoolean(bool value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value ? "true" : "false";
        }

        public override bool ValueEquals(JsonValue obj)
        {
            return obj is JsonBoolean boolean && boolean.value == value;
        }

        public override void Accept(IJsonVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class JsonArray : JsonValue
    {
        public List<JsonValue> value;
        public override object GetValue() => value;

        public JsonArray(List<JsonValue> value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            StringBuilder result = new();
            result.Append('[');
            bool first = true;
            foreach (var item in value)
            {
                if (!first) result.Append(',');
                first = false;
                result.Append(item.ToString());
            }
            result.Append(']');
            return result.ToString();
        }

        public override bool ValueEquals(JsonValue obj)
        {
            if (obj is JsonArray arr && arr.value.Count == value.Count)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    if (!value[i].ValueEquals(arr.value[i])) return false;
                }
                return true;
            }
            return false;
        }

        public override void Accept(IJsonVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class JsonObject : JsonValue
    {
        public Dictionary<string, JsonValue> value;
        public override object GetValue() => value;

        public JsonObject(Dictionary<string, JsonValue> value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            StringBuilder result = new();
            result.Append('{');
            bool first = true;
            foreach (var pair in value)
            {
                if (!first) result.Append(',');
                first = false;
                result.Append('"');
                result.Append(StringUtils.EscapeJsonString(pair.Key));
                result.Append('"');
                result.Append(':');
                result.Append(pair.Value.ToString());
            }
            result.Append('}');
            return result.ToString();
        }

        public override bool ValueEquals(JsonValue obj)
        {
            if (obj is JsonObject obj2 && obj2.value.Count == value.Count)
            {
                foreach (var (key, val) in value)
                {
                    if (!obj2.value.TryGetValue(key, out var val2) || !val.ValueEquals(val2)) return false;
                }
                return true;
            }
            return false;
        }

        public override void Accept(IJsonVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class JsonNull : JsonValue
    {
        public override object GetValue() => null;

        public JsonNull() { }

        public override string ToString()
        {
            return "null";
        }

        public override bool ValueEquals(JsonValue obj)
        {
            return obj is JsonNull;
        }

        public override void Accept(IJsonVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public interface IJsonVisitor
    {
        public void Visit(JsonString json);
        public void Visit(JsonNumber json);
        public void Visit(JsonBoolean json);
        public void Visit(JsonArray json);
        public void Visit(JsonObject json);
        public void Visit(JsonNull json);
    }

    public interface IJsonVisitor<T> : IJsonVisitor
    {
        public T GetResult();
    }
}