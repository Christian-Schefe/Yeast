using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Yeast.Ion;
using Yeast.Json;

public class JsonTest
{
    [Test]
    public void TestSimpleStructStringify()
    {
        Person john = new("John", 25, "Swimming", "Reading");
        string json = Json.Stringify(john);
        Assert.AreEqual("{\"name\":\"John\",\"age\":25,\"hobbies\":[\"Swimming\",\"Reading\"],\"<Prop>k__BackingField\":\"John25\"}", json);
    }

    [Test]
    public void TestPrimitivesStringify()
    {
        Assert.AreEqual("1", Json.Stringify((byte)1));
        Assert.AreEqual("1", Json.Stringify((short)1));
        Assert.AreEqual("99", Json.Stringify('c'));
        Assert.AreEqual("1", Json.Stringify(1));
        Assert.AreEqual("1.5", Json.Stringify(1.5));
        Assert.AreEqual("true", Json.Stringify(true));
        Assert.AreEqual("false", Json.Stringify(false));
        Assert.AreEqual("null", Json.Stringify<object>(null));
        Assert.AreEqual("\"Hello\"", Json.Stringify("Hello"));
    }

    [Test]
    public void TestSpecialStringsStringify()
    {
        Assert.AreEqual("\"\\0\"", Json.Stringify("\0"));
        Assert.AreEqual("\"\\u0001\"", Json.Stringify("\x01"));
        Assert.AreEqual("\"\\n\"", Json.Stringify("\n"));
        Assert.AreEqual("\"\\r\"", Json.Stringify("\r"));
        Assert.AreEqual("\"\\t\"", Json.Stringify("\t"));
        Assert.AreEqual("\"\\b\"", Json.Stringify("\b"));
        Assert.AreEqual("\"\\\\\"", Json.Stringify("\\"));
        Assert.AreEqual("\"\\\"\"", Json.Stringify("\""));

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
    public void TestArraysStringify()
    {
        Assert.AreEqual("[1,2,3]", Json.Stringify(new int[] { 1, 2, 3 }));
        Assert.AreEqual("[1,2.0999999046325684,3.4440000057220459]", Json.Stringify(new float[] { 1f, 2.1f, 3.444f }));
        Assert.AreEqual("[\"Hello\",\"World\"]", Json.Stringify(new string[] { "Hello", "World" }));
        Assert.AreEqual("[1,2,3]", Json.Stringify(new List<int>() { 1, 2, 3 }));
        Assert.AreEqual("[{\"key\":1,\"value\":3},{\"key\":2,\"value\":4},{\"key\":4,\"value\":5}]", Json.Stringify(new Dictionary<int, int>() { { 1, 3 }, { 2, 4 }, { 4, 5 } }));
    }

    [Test]
    public void TestPrimitivesParse()
    {
        Assert.AreEqual(1, Json.Parse<int>("1"));
        Assert.AreEqual(1.5f, Json.Parse<float>("1.5"));
        Assert.AreEqual(true, Json.Parse<bool>("true"));
        Assert.AreEqual(false, Json.Parse<bool>("false"));
        Assert.AreEqual(null, Json.Parse<object>("null"));
        Assert.AreEqual("Hello", Json.Parse<string>("\"Hello\""));
    }

    [Test]
    public void TestArraysParse()
    {
        Assert.AreEqual(new int[] { 1, 2, 3 }, Json.Parse<int[]>("[1,2,3]"));
        Assert.AreEqual(new float[] { 1f, 2.1f, 3.444f }, Json.Parse<float[]>("[1,2.0999999046325684,3.4440000057220459]"));
        Assert.AreEqual(new string[] { "Hello", "World" }, Json.Parse<string[]>("[\"Hello\",\"World\"]"));
        Assert.AreEqual(new List<int>() { 1, 2, 3 }, Json.Parse<List<int>>("[1,2,3]"));


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
        Assert.AreEqual(5, Json.Parse<object>("5"));
        Assert.AreEqual(5.5, Json.Parse<object>("5.5"));
        Assert.AreEqual("hello", Json.Parse<object>("\"hello\""));
        Assert.AreEqual(true, Json.Parse<object>("true"));
        Assert.AreEqual(null, Json.Parse<object>("null"));
        Assert.AreEqual(new object[] { 1, 2, 3 }, Json.Parse<object>("[1,2,3]"));
        Assert.AreEqual(new Dictionary<string, object>() { { "a", 1 }, { "b", 3 } }, Json.Parse<object>("{\"a\":1,\"b\":3}"));
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
    public void TestIgnoreExtraFields()
    {
        Assert.AreEqual(new SimpleClass(1, "Hello"), Json.Parse<SimpleClass>("{\"number\":1,\"str\":\"Hello\",\"extra\":5}", JsonParseMode.Loose));
        Assert.Throws<TypeMismatchException>(() => Json.Parse<SimpleClass>("{\"number\":1,\"str\":\"Hello\",\"extra\":5}", JsonParseMode.Exact));
    }

    [Test]
    public void TestDefaultsForMissingFields()
    {
        Assert.AreEqual(new SimpleClass(1, null), Json.Parse<SimpleClass>("{\"number\":1,\"extra\":5}", JsonParseMode.Loose));
        Assert.AreEqual(new SimpleClass(0, "hello"), Json.Parse<SimpleClass>("{\"str\":\"hello\"}", JsonParseMode.Loose));
        Assert.Throws<TypeMismatchException>(() => Json.Parse<SimpleClass>("{\"number\":1}", JsonParseMode.Exact));
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
        Assert.Throws<CircularReferenceException>(() => Json.Stringify(new Circular(5)));
        Assert.Throws<CircularReferenceException>(() => Json.Stringify(new SingleNested(500)));
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
        var json = Json.Stringify(new Person("John", 25, "Swimming", "Reading"), JsonStringifyMode.Pretty);
        Debug.Log(json);

        json = Json.Stringify(new Nested(2), JsonStringifyMode.Pretty);
        Debug.Log(json);

        json = Json.Stringify(new List<Nested>() { new(2), new(1), new(0) }, JsonStringifyMode.Pretty);
        Debug.Log(json);

        json = Json.Stringify(new MinimalPrettyTest { number = 4, arr = new string[] { "hello world", "hi", "hello" } }, JsonStringifyMode.Pretty);
        Assert.AreEqual("{\n  \"number\": 4,\n  \"arr\": [\n    \"hello world\",\n    \"hi\",\n    \"hello\"\n  ]\n}", json);
    }

    private void Test<T>(T obj)
    {
        var json = Json.Stringify(obj);
        Debug.Log(json);
        Assert.AreEqual(obj, Json.Parse<T>(json));
    }
}

public struct MinimalPrettyTest
{
    public int number;
    public string[] arr;
}

public class Circular
{
    public Circular child;
    public int value;

    public Circular(int value)
    {
        this.value = value;
        child = this;
    }

    public Circular() { }

    public override bool Equals(object obj)
    {
        return obj is Circular circular && value == circular.value &&
               EqualityComparer<Circular>.Default.Equals(child, circular.child);
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(child, value);
    }
}

public class Generic<T>
{

    public T value;

    public Generic(T value)
    {
        this.value = value;
    }

    public Generic() { }

    public override string ToString()
    {
        return value.ToString();
    }

    public override bool Equals(object obj)
    {
        return obj is Generic<T> generic && value.Equals(generic.value);
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(value);
    }
}

public class SimpleClass
{
    public int number;
    public string str;

    public SimpleClass(int number, string str)
    {
        this.number = number;
        this.str = str;
    }

    public SimpleClass() { }

    public override bool Equals(object obj)
    {
        return obj is SimpleClass @class &&
               number == @class.number &&
               str == @class.str;
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(number, str);
    }
}

public class SingleNested
{
    public SingleNested child;

    public int depth;
    public SingleNested() { }
    public SingleNested(int d)
    {
        depth = d;
        if (d > 0)
        {
            child = new SingleNested(d - 1);
        }
    }

    public override bool Equals(object obj)
    {
        return obj is SingleNested nested && depth == nested.depth &&
               EqualityComparer<SingleNested>.Default.Equals(child, nested.child);
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(child, depth);
    }
}

public class Nested
{
    public Nested childL;
    public Nested childR;
    private int depth;

    public Nested(int depth)
    {
        if (depth > 0)
        {
            childL = new Nested(depth - 1);
            childR = new Nested(depth - 1);
        }
        this.depth = depth;
    }

    public Nested() { }

    public override bool Equals(object obj)
    {
        return obj is Nested nested && depth == nested.depth &&
               EqualityComparer<Nested>.Default.Equals(childL, nested.childL) &&
               EqualityComparer<Nested>.Default.Equals(childR, nested.childR);
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(childL, childR, depth);
    }
}

public enum TestEnum
{
    A, B, C, None
}

public enum TestEnumShort : short
{
    A, B, C, None
}

public struct Person
{
    public string name;
    public int age;
    public string[] hobbies;
    public string Prop { get; set; }
    [System.NonSerialized] private string doesntMatter;

    public Person(string name, int age, params string[] hobbies)
    {
        this.name = name;
        this.age = age;
        this.hobbies = hobbies;
        Prop = name + age;
        doesntMatter = "test";
    }

    public readonly override string ToString()
    {
        return $"Person({name}, {age}, {string.Join(", ", hobbies)}), {Prop}, {doesntMatter}";
    }

    public readonly override bool Equals(object obj)
    {
        return obj is Person person &&
               name == person.name &&
               age == person.age && Prop == person.Prop &&
               System.Linq.Enumerable.SequenceEqual(hobbies, person.hobbies);
    }

    public readonly override int GetHashCode()
    {
        return System.HashCode.Combine(name, age, hobbies, Prop);
    }
}
