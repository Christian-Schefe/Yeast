using System.Collections.Generic;
using Yeast.Memento;
using Yeast.Utils;

namespace Yeast.Json
{
    public class ToJsonMementoVisitor : IMementoVisitor<JsonValue>
    {
        public JsonValue result;

        public JsonValue GetResult()
        {
            return result;
        }

        public void Visit(NullMemento memento)
        {
            result = new JsonNull();
        }

        public void Visit(StringMemento memento)
        {
            result = new JsonString(memento.value);
        }

        public void Visit(IntegerMemento memento)
        {
            result = new JsonNumber(memento.value);
        }

        public void Visit(DecimalMemento memento)
        {
            result = new JsonNumber(memento.value);
        }

        public void Visit(BoolMemento memento)
        {
            result = new JsonBoolean(memento.value);
        }

        public void Visit(ArrayMemento memento)
        {
            var arr = new List<JsonValue>();
            foreach (var item in memento.value)
            {
                item.Accept(this);
                arr.Add(result);
            }
            result = new JsonArray(arr);
        }

        public void Visit(DictMemento memento)
        {
            var obj = new Dictionary<string, JsonValue>();
            foreach (var pair in memento.value)
            {
                pair.Value.Accept(this);
                obj.Add(pair.Key, result);
            }
            result = new JsonObject(obj);
        }
    }

    public class ToStringMementoJsonVisitor : IJsonVisitor<IMemento>
    {
        public StringMemento result;

        public IMemento GetResult()
        {
            return result;
        }

        public void Visit(JsonString json)
        {
            result = new StringMemento(json.value);
        }

        public void Visit(JsonNumber json)
        {
            result = new StringMemento(json.ToString());
        }

        public void Visit(JsonBoolean json)
        {
            result = new StringMemento(json.ToString());
        }

        public void Visit(JsonArray json)
        {
            throw new System.InvalidOperationException("Cannot convert JsonArray to StringMemento");
        }

        public void Visit(JsonObject json)
        {
            throw new System.InvalidOperationException("Cannot convert JsonObject to StringMemento");
        }

        public void Visit(JsonNull json)
        {
            result = new StringMemento(null);
        }
    }

    public class ToIntegerMementoJsonVisitor : IJsonVisitor<IMemento>
    {
        public IntegerMemento result;

        public IMemento GetResult()
        {
            return result;
        }

        public void Visit(JsonString json)
        {
            result = new IntegerMemento(StringUtils.StringToLong(json.value));
        }

        public void Visit(JsonNumber json)
        {
            result = new IntegerMemento((long)json.value);
        }

        public void Visit(JsonBoolean json)
        {
            result = new IntegerMemento(json.value ? 1 : 0);
        }

        public void Visit(JsonArray json)
        {
            throw new System.InvalidOperationException("Cannot convert JsonArray to StringMemento");
        }

        public void Visit(JsonObject json)
        {
            throw new System.InvalidOperationException("Cannot convert JsonObject to StringMemento");
        }

        public void Visit(JsonNull json)
        {
            throw new System.InvalidOperationException("Cannot convert JsonNull to IntegerMemento");
        }
    }

    public class ToDecimalMementoJsonVisitor : IJsonVisitor<IMemento>
    {
        public DecimalMemento result;

        public IMemento GetResult()
        {
            return result;
        }

        public void Visit(JsonString json)
        {
            result = new DecimalMemento(StringUtils.StringToDouble(json.value));
        }

        public void Visit(JsonNumber json)
        {
            result = new DecimalMemento(json.value);
        }

        public void Visit(JsonBoolean json)
        {
            result = new DecimalMemento(json.value ? 1 : 0);
        }

        public void Visit(JsonArray json)
        {
            throw new System.InvalidOperationException("Cannot convert JsonArray to StringMemento");
        }

        public void Visit(JsonObject json)
        {
            throw new System.InvalidOperationException("Cannot convert JsonObject to StringMemento");
        }

        public void Visit(JsonNull json)
        {
            throw new System.InvalidOperationException("Cannot convert JsonNull to DecimalMemento");
        }
    }

    public class ToBoolMementoJsonVisitor : IJsonVisitor<IMemento>
    {
        public BoolMemento result;

        public IMemento GetResult()
        {
            return result;
        }

        public void Visit(JsonString json)
        {
            result = json.value switch
            {
                "true" => new BoolMemento(true),
                "false" => new BoolMemento(false),
                _ => throw new System.InvalidOperationException("Cannot convert JsonString to BoolMemento")
            };
        }

        public void Visit(JsonNumber json)
        {
            result = new BoolMemento(json.value != 0);
        }

        public void Visit(JsonBoolean json)
        {
            result = new BoolMemento(json.value);
        }

        public void Visit(JsonArray json)
        {
            throw new System.InvalidOperationException("Cannot convert JsonArray to StringMemento");
        }

        public void Visit(JsonObject json)
        {
            throw new System.InvalidOperationException("Cannot convert JsonObject to StringMemento");
        }

        public void Visit(JsonNull json)
        {
            throw new System.InvalidOperationException("Cannot convert JsonNull to BoolMemento");
        }
    }
}
