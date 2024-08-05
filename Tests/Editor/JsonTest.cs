using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Yeast.Json;

namespace Yeast.Test.Json
{
    public class JsonTest
    {
        [Test]
        public void TestToString()
        {
            var nullVal = new JsonNull();
            Assert.AreEqual("null", nullVal.ToString());

            var boolVal = new JsonBoolean(true);
            Assert.AreEqual("true", boolVal.ToString());

            var intVal = new JsonNumber(42);
            Assert.AreEqual("42", intVal.ToString());

            var floatVal = new JsonNumber(42.42f);
            Assert.AreEqual("42.419998168945313", floatVal.ToString());

            var doubleVal = new JsonNumber(42.42);
            Assert.AreEqual("42.42", doubleVal.ToString());

            var stringVal = new JsonString("Hello, World!");
            Assert.AreEqual("\"Hello, World!\"", stringVal.ToString());

            var arrayVal = new JsonArray(new List<JsonValue> { new JsonNumber(42), new JsonString("Hello, World!") });
            Assert.AreEqual("[42,\"Hello, World!\"]", arrayVal.ToString());

            var objectVal = new JsonObject(new Dictionary<string, JsonValue> { { "int", new JsonNumber(42) }, { "string", new JsonString("Hello, World!") } });
            Assert.AreEqual("{\"int\":42,\"string\":\"Hello, World!\"}", objectVal.ToString());
        }

        [Test]
        public void TestParse()
        {
            var nullVal = JsonValue.FromString("null");
            Assert.IsTrue(new JsonNull().ValueEquals(nullVal));

            var boolVal = JsonValue.FromString("true");
            Assert.IsTrue(new JsonBoolean(true).ValueEquals(boolVal));

            var intVal = JsonValue.FromString("42");
            Assert.IsTrue(new JsonNumber(42).ValueEquals(intVal));

            var floatVal = JsonValue.FromString("42.419998168945313");
            Assert.IsTrue(new JsonNumber(42.42f).ValueEquals(floatVal));

            var doubleVal = JsonValue.FromString("42.42");
            Assert.IsTrue(new JsonNumber(42.42).ValueEquals(doubleVal));

            var stringVal = JsonValue.FromString("\"Hello, World!\"");
            Assert.IsTrue(new JsonString("Hello, World!").ValueEquals(stringVal));

            var arrayVal = JsonValue.FromString("[42,\"Hello, World!\"]");
            Assert.IsTrue(new JsonArray(new List<JsonValue> { new JsonNumber(42), new JsonString("Hello, World!") }).ValueEquals(arrayVal));

            var objectVal = JsonValue.FromString("{\"int\":42,\"string\":\"Hello, World!\"}");
            Assert.IsTrue(new JsonObject(new Dictionary<string, JsonValue> { { "int", new JsonNumber(42) }, { "string", new JsonString("Hello, World!") } }).ValueEquals(objectVal));
        }

        [Test]
        public void TestRoundtrip()
        {
            Test(new JsonNull());
            Test(new JsonBoolean(true));
            Test(new JsonBoolean(false));
            Test(new JsonNumber(42));
            Test(new JsonNumber(42.42f));
            Test(new JsonNumber(42.42));
            Test(new JsonString("Hello, World!"));
            Test(new JsonArray(new List<JsonValue> { new JsonNumber(42), new JsonString("Hello, World!") }));
            Test(new JsonObject(new Dictionary<string, JsonValue> { { "int", new JsonNumber(42) }, { "string", new JsonString("Hello, World!") } }));
            Test(new JsonString("All JSON special characters: \"\\/\b\f\n\r\t\u0000\uFFFF\u4E35. Other special characters: -.,;:_<>|#'+*~´`ß?=})]([{&%$§³²!^°@€êáù"));
        }

        private void Test<T>(T obj) where T : JsonValue
        {
            var json = obj.ToString();
            UnityEngine.Debug.Log(json);
            var parsed = JsonValue.FromString(json);
            UnityEngine.Debug.Log(parsed.GetValue());
            Assert.IsTrue(obj.ValueEquals(parsed));
        }
    }
}