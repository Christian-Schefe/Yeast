using System.Collections.Generic;
using NUnit.Framework;
using Yeast.Memento;

namespace Yeast.Test
{
    public class MementoTest
    {
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
            Test(15m);
            Test(decimal.MaxValue);
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
        public void TestComplicatedArrays()
        {
            var b0 = new int[,] { { 0, 3 } };
            var b1 = new int[,] { { 1, 2, 3, 2 }, { 1, 2, 3, 2 } };
            var b2 = new int[,] { { 8, 9 }, { 0, -1 }, { 8, 9 } };
            var arr = new int[,,][,] { { { b1, b2, b2 }, { b1, b0, b0 } }, { { b2, b1, b0 }, { b2, b0, b1 } } };

            var c = new int[][,,][,] { arr, arr, arr };
            Test(c);
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
            var converter = new MementoConverter();
            var result = converter.Serialize(val);

            UnityEngine.Debug.Log($"Memento: {result}");
            var val2 = converter.Deserialize(type, result);
            Assert.AreEqual(val, val2);

            var result2 = converter.Serialize(val2);
            Assert.IsTrue(result.ValueEquals(result2));
        }
    }
}
