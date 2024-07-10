using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class JsonTest
{
    [Test]
    public void TestSimpleStructStringify()
    {
        Person john = new("John", 25, "Swimming", "Reading");
        string json = Yeast.Stringify(john);
        Assert.AreEqual("{\"name\":\"John\",\"age\":25,\"hobbies\":[\"Swimming\",\"Reading\"],\"<Prop>k__BackingField\":\"John25\"}", json);
    }

    [Test]
    public void TestPrimitivesStringify()
    {
        Assert.AreEqual("1", Yeast.Stringify((byte)1));
        Assert.AreEqual("1", Yeast.Stringify((short)1));
        Assert.AreEqual("99", Yeast.Stringify('c'));
        Assert.AreEqual("1", Yeast.Stringify(1));
        Assert.AreEqual("1.5", Yeast.Stringify(1.5));
        Assert.AreEqual("true", Yeast.Stringify(true));
        Assert.AreEqual("false", Yeast.Stringify(false));
        Assert.AreEqual("null", Yeast.Stringify<object>(null));
        Assert.AreEqual("\"Hello\"", Yeast.Stringify("Hello"));
    }

    [Test]
    public void TestSpecialStringsStringify()
    {
        Assert.AreEqual("\"\\0\"", Yeast.Stringify("\0"));
        Assert.AreEqual("\"\\u0001\"", Yeast.Stringify("\x01"));
        Assert.AreEqual("\"\\n\"", Yeast.Stringify("\n"));
        Assert.AreEqual("\"\\r\"", Yeast.Stringify("\r"));
        Assert.AreEqual("\"\\t\"", Yeast.Stringify("\t"));
        Assert.AreEqual("\"\\b\"", Yeast.Stringify("\b"));
        Assert.AreEqual("\"\\\\\"", Yeast.Stringify("\\"));
        Assert.AreEqual("\"\\\"\"", Yeast.Stringify("\""));
    }

    [Test]
    public void TestArraysStringify()
    {
        Assert.AreEqual("[1,2,3]", Yeast.Stringify(new int[] { 1, 2, 3 }));
        Assert.AreEqual("[1,2.0999999046325684,3.4440000057220459]", Yeast.Stringify(new float[] { 1f, 2.1f, 3.444f }));
        Assert.AreEqual("[\"Hello\",\"World\"]", Yeast.Stringify(new string[] { "Hello", "World" }));
        Assert.AreEqual("[1,2,3]", Yeast.Stringify(new List<int>() { 1, 2, 3 }));
        Assert.AreEqual("[{\"key\":1,\"value\":3},{\"key\":2,\"value\":4},{\"key\":4,\"value\":5}]", Yeast.Stringify(new Dictionary<int, int>() { { 1, 3 }, { 2, 4 }, { 4, 5 } }));
    }

    [Test]
    public void TestPrimitivesParse()
    {
        Yeast.PushSettings(ConversionSettings.Strict);
        Assert.AreEqual(1, Yeast.Parse<int>("1"));
        Assert.AreEqual(1.5f, Yeast.Parse<float>("1.5"));
        Assert.AreEqual(true, Yeast.Parse<bool>("true"));
        Assert.AreEqual(false, Yeast.Parse<bool>("false"));
        Assert.AreEqual(null, Yeast.Parse<object>("null"));
        Assert.AreEqual("Hello", Yeast.Parse<string>("\"Hello\""));
        Yeast.PopSettings();
    }

    [Test]
    public void TestArraysParse()
    {
        Yeast.PushSettings(ConversionSettings.Strict);
        Assert.AreEqual(new int[] { 1, 2, 3 }, Yeast.Parse<int[]>("[1,2,3]"));
        Assert.AreEqual(new float[] { 1f, 2.1f, 3.444f }, Yeast.Parse<float[]>("[1,2.0999999046325684,3.4440000057220459]"));
        Assert.AreEqual(new string[] { "Hello", "World" }, Yeast.Parse<string[]>("[\"Hello\",\"World\"]"));
        Assert.AreEqual(new List<int>() { 1, 2, 3 }, Yeast.Parse<List<int>>("[1,2,3]"));


        var list = new List<int>();
        for (int i = 0; i < 10000; i++)
        {
            list.Add(i);
        }
        Test(list);
        Yeast.PopSettings();
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
        Assert.AreEqual(5, Yeast.Parse<object>("5"));
        Assert.AreEqual(5.5, Yeast.Parse<object>("5.5"));
        Assert.AreEqual("hello", Yeast.Parse<object>("\"hello\""));
        Assert.AreEqual(true, Yeast.Parse<object>("true"));
        Assert.AreEqual(null, Yeast.Parse<object>("null"));
        Assert.AreEqual(new object[] { 1, 2, 3 }, Yeast.Parse<object>("[1,2,3]"));
        Assert.AreEqual(new Dictionary<string, object>() { { "a", 1 }, { "b", 3 } }, Yeast.Parse<object>("{\"a\":1,\"b\":3}"));
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
        Assert.AreEqual(new SimpleClass(1, "Hello"), Yeast.Parse<SimpleClass>("{\"number\":1,\"str\":\"Hello\",\"extra\":5}"));
        Assert.Throws<ConversionExceptions.TypeMismatchException>(() => Yeast.Parse<SimpleClass>("{\"number\":1,\"str\":\"Hello\",\"extra\":5}", ConversionSettings.Strict));
    }

    [Test]
    public void TestDefaultsForMissingFields()
    {
        Yeast.PushSettings(ConversionSettings.Relaxed);
        Assert.AreEqual(new SimpleClass(1, null), Yeast.Parse<SimpleClass>("{\"number\":1,\"extra\":5}"));
        Assert.AreEqual(new SimpleClass(0, "hello"), Yeast.Parse<SimpleClass>("{\"str\":\"hello\"}"));
        Assert.Throws<ConversionExceptions.TypeMismatchException>(() => Yeast.Parse<SimpleClass>("{\"number\":1}", ConversionSettings.Strict));
        Yeast.PopSettings();
    }

    private void Test<T>(T obj)
    {
        var json = Yeast.Stringify(obj);
        Debug.Log(json);
        Assert.AreEqual(obj, Yeast.Parse<T>(json));
    }
}

public class Generic<T>
{
    public T value;

    public Generic(T value)
    {
        this.value = value;
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
