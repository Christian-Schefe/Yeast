using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Yeast.Ion;
using Yeast.Json;

namespace Yeast.Test
{
    public class JsonTest
    {
        [Test]
        public void TestPrimitivesStringify()
        {
            Assert.AreEqual("1", JSON.Stringify((byte)1));
            Assert.AreEqual("1", JSON.Stringify((short)1));
            Assert.AreEqual("99", JSON.Stringify('c'));
            Assert.AreEqual("1", JSON.Stringify(1));
            Assert.AreEqual("1.5", JSON.Stringify(1.5));
            Assert.AreEqual("true", JSON.Stringify(true));
            Assert.AreEqual("false", JSON.Stringify(false));
            Assert.AreEqual("null", JSON.Stringify<object>(null));
            Assert.AreEqual("\"Hello\"", JSON.Stringify("Hello"));
        }

        [Test]
        public void TestSpecialStringsStringify()
        {
            Assert.AreEqual("\"\\0\"", JSON.Stringify("\0"));
            Assert.AreEqual("\"\\u0001\"", JSON.Stringify("\x01"));
            Assert.AreEqual("\"\\n\"", JSON.Stringify("\n"));
            Assert.AreEqual("\"\\r\"", JSON.Stringify("\r"));
            Assert.AreEqual("\"\\t\"", JSON.Stringify("\t"));
            Assert.AreEqual("\"\\b\"", JSON.Stringify("\b"));
            Assert.AreEqual("\"\\\\\"", JSON.Stringify("\\"));
            Assert.AreEqual("\"\\\"\"", JSON.Stringify("\""));

            Test("\0");
            Test("\x01");
            Test("\0\a\b\f\r\t\n\v\u1234");
            Test("\n");
            Test("\r");
            Test("\t");
            Test("\b");
            Test("\\");
            Test("\"");
        }

        [Test]
        public void TestBigNumbers()
        {
            Test(1_000_000_000_000_000);
            Test(1_000_000_000_000_000_000_000f);
            Test(-1_000_000_000_000_000_000_000d);
            Test(double.Epsilon);
            Test(double.MaxValue);
            Test(double.NaN);
            Test(double.NegativeInfinity);
            Test(double.PositiveInfinity);
            Test(decimal.MaxValue);
            Test(15m);
            Test(decimal.MinValue);
        }

        [Test]
        public void TestArrays()
        {
            Test(new int[] { 1, 2, 3 });
            Test(new float[] { 1f, 2.1f, 3.444f });
            Test(new int[,] { { 1, 2 }, { 3, 4 } });
            Test(new string[] { "Hello", "World" });
            Test(new List<int>() { 1, 2, 3 });
            Test(new List<float>() { 1f, 2.1f, 3.444f });
            Test(new List<string>() { "Hello", "World" });
            Test(new List<List<int>>() { new() { 1, 2 }, new() { 3, 4 } });
        }
        /*

        [Test]
        public void TestArraysStringify()
        {
            Assert.AreEqual("[1,2,3]", JSON.Stringify(new int[] { 1, 2, 3 }));
            Assert.AreEqual("[1.0,2.0999999046325684,3.4440000057220459]", JSON.Stringify(new float[] { 1f, 2.1f, 3.444f }));
            Assert.AreEqual("[\"Hello\",\"World\"]", JSON.Stringify(new string[] { "Hello", "World" }));
            Assert.AreEqual("[1,2,3]", JSON.Stringify(new List<int>() { 1, 2, 3 }));
            Assert.AreEqual("[{\"key\":1,\"value\":3},{\"key\":2,\"value\":4},{\"key\":4,\"value\":5}]", JSON.Stringify(new Dictionary<int, int>() { { 1, 3 }, { 2, 4 }, { 4, 5 } }));
        }

        [Test]
        public void TestPrimitivesParse()
        {
            Assert.AreEqual(1, JSON.Parse<int>("1"));
            Assert.AreEqual(1.5f, JSON.Parse<float>("1.5"));
            Assert.AreEqual(true, JSON.Parse<bool>("true"));
            Assert.AreEqual(false, JSON.Parse<bool>("false"));
            Assert.AreEqual(null, JSON.Parse<object>("null"));
            Assert.AreEqual("Hello", JSON.Parse<string>("\"Hello\""));
        }

        [Test]
        public void TestArraysParse()
        {
            Assert.AreEqual(new int[] { 1, 2, 3 }, JSON.Parse<int[]>("[1,2,3]"));
            Assert.AreEqual(new float[] { 1f, 2.1f, 3.444f }, JSON.Parse<float[]>("[1.0,2.0999999046325684,3.4440000057220459]"));
            Assert.AreEqual(new string[] { "Hello", "World" }, JSON.Parse<string[]>("[\"Hello\",\"World\"]"));
            Assert.AreEqual(new List<int>() { 1, 2, 3 }, JSON.Parse<List<int>>("[1,2,3]"));


            var list = new List<int>();
            for (int i = 0; i < 10000; i++)
            {
                list.Add(i);
            }
            Test(list);
        }

        [Test]
        public void TestVarious()
        {
            Test("Hello");
            Test(1);
            Test(1.5f);
            Test(true);
            Test(false);
            Test<object>(null);
            Test(new int[] { 1, 2, 3 });
            Test(new float[] { 1f, 2.1f, 3.444f });
            Test(new string[] { "Hello", "World" });
            Test(new List<int>() { 1, 2, 3 });
            Test(new Person("John", 25, "Swimming", "Reading"));
            Test(new KeyValuePair<int, int>(1, 3));
            Test(new Dictionary<int, int>() { { 1, 3 }, { 2, 4 }, { 4, 5 } });
            Test("\n\t\r\b\u0001\0");
            Test(new Nested(3));
        }

        [Test]
        public void TestEnums()
        {
            Test(TestEnum.A);
            Test(TestEnum.B);
            Test(TestEnum.C);
            Test(TestEnum.None);
            Test(TestEnumShort.A);
            Test(TestEnumShort.B);
            Test(TestEnumShort.C);
            Test(TestEnumShort.None);
        }

        [Test]
        public void TestObjectType()
        {
            Assert.AreEqual(5, JSON.Parse<object>("5"));
            Assert.AreEqual(5.5, JSON.Parse<object>("5.5"));
            Assert.AreEqual("hello", JSON.Parse<object>("\"hello\""));
            Assert.AreEqual(true, JSON.Parse<object>("true"));
            Assert.AreEqual(null, JSON.Parse<object>("null"));
            Assert.AreEqual(new object[] { 1, 2, 3 }, JSON.Parse<object>("[1,2,3]"));
            Assert.AreEqual(new Dictionary<string, object>() { { "a", 1 }, { "b", 3 } }, JSON.Parse<object>("{\"a\":1,\"b\":3}"));
        }

        [Test]
        public void TestIgnoreExtraFields()
        {
            Assert.AreEqual(new SimpleClass(1, "Hello"), JSON.Parse<SimpleClass>("{\"number\":1,\"str\":\"Hello\",\"extra\":5}", JsonParseMode.Loose));
            Assert.Throws<TypeMismatchException>(() => JSON.Parse<SimpleClass>("{\"number\":1,\"str\":\"Hello\",\"extra\":5}", JsonParseMode.Exact));
        }

        [Test]
        public void TestDefaultsForMissingFields()
        {
            Assert.AreEqual(new SimpleClass(1, null), JSON.Parse<SimpleClass>("{\"number\":1,\"extra\":5}", JsonParseMode.Loose));
            Assert.AreEqual(new SimpleClass(0, "hello"), JSON.Parse<SimpleClass>("{\"str\":\"hello\"}", JsonParseMode.Loose));
            Assert.Throws<TypeMismatchException>(() => JSON.Parse<SimpleClass>("{\"number\":1}", JsonParseMode.Exact));
        }

        [Test]
        public void TestNullable()
        {
            Test<int?>(null);
            Test<int?>(5);
            Test<Person?>(null);
            Test<Person?>(new Person("John", 25, "Swimming", "Reading"));
            Test<TestEnum?>(null);
            Test<TestEnum?>(TestEnum.A);
        }

        [Test]
        public void TestCircular()
        {
            Assert.Throws<CircularReferenceException>(() => JSON.Stringify(new Circular(5)));
            Assert.Throws<CircularReferenceException>(() => JSON.Stringify(new SingleNested(500)));
            Test(new SingleNested(99));
        }

        [Test]
        public void TestGeneric()
        {
            Test(new Generic<int>(5));
            Test(new Generic<string>("Hello"));
            Test(new Generic<Person>(new Person("John", 25, "Swimming", "Reading")));
            Test(new Generic<object>(5L));
            Test(new Generic<object>("Hello"));
        }

        [Test]
        public void TestPrettyPrint()
        {
            var json = JSON.Stringify(new Person("John", 25, "Swimming", "Reading"), JsonStringifyMode.Pretty);
            Debug.Log(json);

            json = JSON.Stringify(new Nested(2), JsonStringifyMode.Pretty);
            Debug.Log(json);

            json = JSON.Stringify(new List<Nested>() { new(2), new(1), new(0) }, JsonStringifyMode.Pretty);
            Debug.Log(json);

            json = JSON.Stringify(new MinimalPrettyTest { number = 4, arr = new string[] { "hello world", "hi", "hello" } }, JsonStringifyMode.Pretty);
            Assert.AreEqual("{\n  \"number\": 4,\n  \"arr\": [\n    \"hello world\",\n    \"hi\",\n    \"hello\"\n  ]\n}", json);
        }*/

        [Test]
        public void TestInheritance()
        {
            var student = new Student("John", 17, 1.75f, 135165);
            Test(student);
        }

        [Test]
        public void TestUnityBuiltins()
        {
            Test(new Vector2(1, 2));
            Test(new Vector3(1, 2, 3));
            Test(new Vector4(1, 2, 3, 4));
            Test(new Quaternion(1, 2, 3, 4));
            Test(new Color(0.5f, 0.5f, 0.5f, 0.5f));
            Test(new Rect(1, 2, 3, 4));
            Test(new Bounds(new Vector3(1, 2, 3), new Vector3(4, 5, 6)));
            Test(new BoundsInt(new Vector3Int(1, 2, 3), new Vector3Int(4, 5, 6)));
            Test(new RectInt(1, 2, 3, 4));
            Test(new Vector2Int(1, 2));
            Test(new Vector3Int(1, 2, 3));
            Test(new Color32(1, 2, 3, 4));
        }

        [Test]
        public void TestSpecialBuiltins()
        {
            Test(System.Guid.NewGuid());
            Test(new System.DateTime(2021, 1, 1, 12, 0, 0));
            Test(new System.TimeSpan(1, 2, 3, 4));
            Test(new System.DateTimeOffset(2021, 1, 1, 12, 0, 0, new System.TimeSpan(0, 2, 3, 0)));
        }

        private void Test<T>(T obj)
        {
            var json = JSON.Stringify(obj);
            UnityEngine.Debug.Log(json);

            var obj2 = JSON.Parse<T>(json);
            Assert.AreEqual(obj, obj2);
            var json2 = JSON.Stringify(obj2);
            Assert.AreEqual(json, json2);
        }
    }
}
