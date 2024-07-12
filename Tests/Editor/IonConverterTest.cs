using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Yeast.Binary;
using Yeast.Ion;

namespace Yeast.Test
{
    public class IonConverterTest
    {
        [Test]
        public void TestAll()
        {
            TestNull();
            TestIntegers();
            TestStrings();
            TestFloats();
            TestBooleans();
            TestArrays();
            TestMaps();
        }

        [Test]
        public void TestNull()
        {
            Test(null);
        }

        [Test]
        public void TestIntegers()
        {
            Test(0);
            Test(1);
            Test(-1);
            Test(624562456);
            Test(-624562456);
            Test(long.MaxValue);
            Test(long.MinValue);
        }

        [Test]
        public void TestStrings()
        {
            Test("Hello World!");
            Test("");
            Test(" ");
            Test("1234567890");
            Test("Hello\nWorld!");
            Test("Hello\t\0\r\\\"World!");
        }

        [Test]
        public void TestFloats()
        {
            Test(0);
            Test(1);
            Test(-1);
            Test(3.14159f);
            Test(-3.14159f);
            Test(3457457457.14159d);
            Test(-3457457457.14159d);
            Test(1E20);
            Test(-1E20);
            Test(float.MaxValue);
            Test(float.MinValue);
        }

        [Test]
        public void TestBooleans()
        {
            Test(true);
            Test(false);
        }

        [Test]
        public void TestArrays()
        {
            Test(new int[] { 4, 2 });
            Test(new int[,] { { 4, 1 }, { 4, 2 } });
            Test(new List<int>());
            Test(new List<int> { 1 });
            Test(new List<int> { 1, 2 });
            Test(new List<int> { 1, 2, 3 });
            Test(new List<object> { 1, "hello", 3.141d, 4 });
            Test(new List<List<int>> {
                new() { 1, 2 },
                new() { 5, 2 },
                new() { 1, 5, 6 }
            });

            var bigList = new List<int>();
            for (int i = 0; i < 1000; i++) bigList.Add(i);
            Test(bigList);
        }

        [Test]
        public void TestMaps()
        {
            Test(new Dictionary<string, int>());
            Test(new Dictionary<string, int> { { "f1", 1 } });
        }

        private void Test(object val)
        {
            var type = val?.GetType() ?? typeof(object);
            var converter = new IonConverter();
            if (!converter.TryInto(val, out var result, new ToIonSettings() { maxDepth = 100 }, out var exception)) throw exception;

            UnityEngine.Debug.Log($"Ion: {result}");
            if (!converter.TryFrom((result, type), out var val2, new FromIonSettings(), out exception)) throw exception;
            Assert.AreEqual(val, val2);

            if (!converter.TryInto(val2, out var result2, new ToIonSettings() { maxDepth = 100 }, out var exception2)) throw exception2;
            Assert.AreEqual(result, result2);
        }
    }
}
