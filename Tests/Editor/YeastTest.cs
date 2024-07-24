using System;
using NUnit.Framework;
using UnityEngine;


namespace Yeast.Test
{
    public class YeastTest
    {
        [Test]
        public void TestIntData()
        {
            foreach (var data in TestDataProvider.GetIntData()) Test(data);
            Test(TestDataProvider.GetIntData());
        }

        [Test]
        public void TestDecimalData()
        {
            foreach (var data in TestDataProvider.GetFloatData()) Test(data);
            Test(TestDataProvider.GetFloatData());

            foreach (var data in TestDataProvider.GetDoubleData()) Test(data);
            Test(TestDataProvider.GetDoubleData());
        }

        [Test]
        public void TestStringData()
        {
            foreach (var data in TestDataProvider.GetStringData()) Test(data);
            Test(TestDataProvider.GetStringData());
        }

        [Test]
        public void TestBoolData()
        {
            foreach (var data in TestDataProvider.GetBoolData()) Test(data);
            Test(TestDataProvider.GetBoolData());
        }

        [Test]
        public void TestSimpleArrayData()
        {
            foreach (var data in TestDataProvider.GetSimpleArrayData()) Test(data);
            Test(TestDataProvider.GetSimpleArrayData());
        }

        [Test]
        public void TestMultidimensionalArrayData()
        {
            foreach (var data in TestDataProvider.GetMultidimensionalArrayData()) Test(data);
            Test(TestDataProvider.GetMultidimensionalArrayData());
        }

        [Test]
        public void TestJaggedMultidimensionalArrayData()
        {
            foreach (var data in TestDataProvider.GetJaggedMultidimensionalArrayData()) Test(data);
            Test(TestDataProvider.GetJaggedMultidimensionalArrayData());
        }

        [Test]
        public void TestListData()
        {
            foreach (var data in TestDataProvider.GetListData()) Test(data);
            Test(TestDataProvider.GetListData());
        }

        [Test]
        public void TestVector2Data()
        {
            foreach (var data in TestDataProvider.GetVector2Data()) Test(data);
            Test(TestDataProvider.GetVector2Data());
        }

        [Test]
        public void TestDictionaryData()
        {
            foreach (var data in TestDataProvider.GetDictionaryData()) Test(data);
            Test(TestDataProvider.GetDictionaryData());
        }

        [Test]
        public void TestHashSetData()
        {
            foreach (var data in TestDataProvider.GetHashSetData()) Test(data);
            Test(TestDataProvider.GetHashSetData());
        }

        [Test]
        public void TestClassData()
        {
            foreach (var data in TestDataProvider.GetClassData()) Test(data);
            Test(TestDataProvider.GetClassData());
        }

        [Test]
        public void TestVariousData()
        {
            foreach (var data in TestDataProvider.GetVariousData()) Test(data.Item1, data.Item2);
        }

        [Test]
        public void TempTest()
        {
            object obj = Yeast.FromJson<int>("66");
            Assert.AreEqual(66, obj);

            obj = Yeast.FromJson<bool>("\"true\"");
            Assert.AreEqual(true, obj);

            obj = Yeast.FromJson<string>("\"null\"");
            Assert.AreEqual("null", obj);

            obj = Yeast.FromJson<string>("null");
            Assert.AreEqual(null, obj);

            obj = Yeast.FromJson<string>("\"null\"");
            Assert.AreEqual("null", obj);

            obj = Yeast.FromJson<string>("55.266e2");
            Assert.AreEqual("5526.6", obj);

            obj = Yeast.FromXml<char>("<root><char>a</char></root>");
            Assert.AreEqual('a', obj);
        }

        private void Test<T>(T obj)
        {
            Test(typeof(T), obj);
        }

        private void Test(Type type, object obj)
        {
            Debug.Log("--- " + type.Name);
            TestJson(type, obj);
            TestBinary(type, obj);
            TestBase64(type, obj);
            TestXml(type, obj);
        }

        private void TestJson(Type type, object obj)
        {
            var json = Yeast.ToJson(obj);
            Debug.Log(json);

            var obj2 = Yeast.FromJson(type, json);
            Assert.AreEqual(obj, obj2);
            var json2 = Yeast.ToJson(obj2);
            Assert.AreEqual(json, json2);
        }

        private void TestBinary(Type type, object obj)
        {
            var bytes = Yeast.ToBytes(obj);
            var hex = "hex " + BitConverter.ToString(bytes);
            Debug.Log(hex);

            var obj2 = Yeast.FromBytes(type, bytes);
            Assert.AreEqual(obj, obj2);
            var bytes2 = Yeast.ToBytes(obj2);
            Assert.AreEqual(bytes, bytes2);
        }

        private void TestBase64(Type type, object obj)
        {
            var base64 = Yeast.ToBase64(obj);
            Debug.Log(base64);

            var obj2 = Yeast.FromBase64(type, base64);
            Assert.AreEqual(obj, obj2);
            var base64_2 = Yeast.ToBase64(obj2);
            Assert.AreEqual(base64, base64_2);
        }

        private void TestXml(Type type, object obj)
        {
            var xml = Yeast.ToXml(obj);
            Debug.Log(xml);

            var obj2 = Yeast.FromXml(type, xml);
            Assert.AreEqual(obj, obj2);
            var xml2 = Yeast.ToXml(obj2);
            Assert.AreEqual(xml, xml2);
        }
    }
}
