using System.Collections.Generic;
using Yeast.Memento;

namespace Yeast.Json
{
    public class JsonConverter : IMementoConverter<JsonValue>
    {
        private class ToMementoJsonVisitor : IJsonVisitor
        {
            public IMemento result;

            public void Visit(JsonString json)
            {
                result = new StringMemento(json.value);
            }

            public void Visit(JsonNumber json)
            {
                result = new DecimalMemento(json.value);
            }

            public void Visit(JsonBoolean json)
            {
                result = new BoolMemento(json.value);
            }

            public void Visit(JsonArray json)
            {
                var arr = new List<IMemento>();
                foreach (var item in json.value)
                {
                    item.Accept(this);
                    arr.Add(result);
                }
                result = new ArrayMemento(arr);
            }

            public void Visit(JsonObject json)
            {
                var obj = new Dictionary<string, IMemento>();
                foreach (var pair in json.value)
                {
                    pair.Value.Accept(this);
                    obj.Add(pair.Key, result);
                }
                result = new DictMemento(obj);
            }

            public void Visit(JsonNull json)
            {
                result = new NullMemento();
            }
        }

        private class ToJsonMementoVisitor : IMementoVisitor
        {
            public JsonValue result;

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

        public IMemento Deserialize(JsonValue value)
        {
            var visitor = new ToMementoJsonVisitor();
            value.Accept(visitor);
            return visitor.result;
        }

        public JsonValue Serialize(IMemento value)
        {
            var visitor = new ToJsonMementoVisitor();
            value.Accept(visitor);
            return visitor.result;
        }
    }
}
