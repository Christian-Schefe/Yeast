using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Yeast.Json;

namespace Yeast.Test
{
    public class JsonTest
    {
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
            Debug.Log(json);

            var obj2 = JSON.Parse<T>(json);
            Assert.AreEqual(obj, obj2);
            var json2 = JSON.Stringify(obj2);
            Assert.AreEqual(json, json2);
        }
    }
}
