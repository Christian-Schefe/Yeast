using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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
            throw new InvalidOperationException("Cannot convert JsonArray to StringMemento");
        }

        public void Visit(JsonObject json)
        {
            throw new InvalidOperationException("Cannot convert JsonObject to StringMemento");
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
            throw new InvalidOperationException("Cannot convert JsonArray to StringMemento");
        }

        public void Visit(JsonObject json)
        {
            throw new InvalidOperationException("Cannot convert JsonObject to StringMemento");
        }

        public void Visit(JsonNull json)
        {
            throw new InvalidOperationException("Cannot convert JsonNull to IntegerMemento");
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
            throw new InvalidOperationException("Cannot convert JsonArray to StringMemento");
        }

        public void Visit(JsonObject json)
        {
            throw new InvalidOperationException("Cannot convert JsonObject to StringMemento");
        }

        public void Visit(JsonNull json)
        {
            throw new InvalidOperationException("Cannot convert JsonNull to DecimalMemento");
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
                _ => throw new InvalidOperationException("Cannot convert JsonString to BoolMemento")
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
            throw new InvalidOperationException("Cannot convert JsonArray to StringMemento");
        }

        public void Visit(JsonObject json)
        {
            throw new InvalidOperationException("Cannot convert JsonObject to StringMemento");
        }

        public void Visit(JsonNull json)
        {
            throw new InvalidOperationException("Cannot convert JsonNull to BoolMemento");
        }
    }

    public class JsonTypeWrapperVisitor : TypeWrapperVisitor<JsonValue, IMemento>
    {
        public override void Visit(StringTypeWrapper stringTypeWrapper)
        {
            var visitor = new ToStringMementoJsonVisitor();
            value.Accept(visitor);
            result = visitor.GetResult();
        }

        public override void Visit(BoolTypeWrapper boolTypeWrapper)
        {
            if (value is JsonNull && boolTypeWrapper.IsNullable)
            {
                result = new NullMemento();
                return;
            }
            var visitor = new ToBoolMementoJsonVisitor();
            value.Accept(visitor);
            result = visitor.GetResult();
        }

        public override void Visit(IntegerTypeWrapper integerTypeWrapper)
        {
            if (value is JsonNull && integerTypeWrapper.IsNullable)
            {
                result = new NullMemento();
                return;
            }
            var visitor = new ToIntegerMementoJsonVisitor();
            value.Accept(visitor);
            result = visitor.GetResult();
        }

        public override void Visit(RationalTypeWrapper rationalTypeWrapper)
        {
            if (value is JsonNull && rationalTypeWrapper.IsNullable)
            {
                result = new NullMemento();
                return;
            }
            var visitor = new ToDecimalMementoJsonVisitor();
            value.Accept(visitor);
            result = visitor.GetResult();
        }

        public override void Visit(CollectionTypeWrapper collectionTypeWrapper)
        {
            if (value is JsonNull)
            {
                result = new NullMemento();
                return;
            }

            void ConvertArray(JsonValue val, int rank)
            {
                if (val is not JsonArray jsonArray)
                {
                    throw new InvalidOperationException($"Cannot convert {val.GetType().Name} to ArrayMemento");
                }

                List<IMemento> list = new();

                foreach (var item in jsonArray.value)
                {
                    if (rank == 1) Convert(item, collectionTypeWrapper.ElementType);
                    else ConvertArray(item, rank - 1);
                    list.Add(result);
                }

                result = new ArrayMemento(list);
            }

            ConvertArray(value, collectionTypeWrapper.Rank);
        }

        public override void Visit(StructTypeWrapper structTypeWrapper)
        {
            if (value is JsonNull)
            {
                result = new NullMemento();
                return;
            }
            if (value is not JsonObject jsonObject)
            {
                throw new InvalidOperationException($"Cannot convert {value.GetType().Name} to DictMemento");
            }

            StructTypeWrapper typeWrapper = structTypeWrapper;

            var obj = new Dictionary<string, IMemento>();

            if (jsonObject.value.TryGetValue("$type", out var typeValue))
            {
                var typeIdentifier = typeValue.AsString();
                if (structTypeWrapper.DerivedTypes.ContainsKey(typeIdentifier))
                {
                    typeWrapper = structTypeWrapper.DerivedTypes[typeIdentifier];
                }
                obj.Add("$type", new StringMemento(typeIdentifier));
            }

            foreach (var pair in jsonObject.value)
            {
                if (pair.Key == "$type") continue;

                var res = Convert(pair.Value, typeWrapper.Fields[pair.Key].type);
                obj.Add(pair.Key, res);
            }

            result = new DictMemento(obj);
        }
    }
}
