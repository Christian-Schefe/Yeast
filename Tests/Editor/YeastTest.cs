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
        public void TestTupleData()
        {
            foreach (var data in TestDataProvider.GetTupleData()) Test(data);
            Test(TestDataProvider.GetTupleData());
        }

        [Test]
        public void TestTypeData()
        {
            foreach (var data in TestDataProvider.GetTypeData()) Test(data);
            Test(TestDataProvider.GetTypeData());
        }

        [Test]
        public void TestVariousData()
        {
            foreach (var data in TestDataProvider.GetVariousData()) Test(data.Item1, data.Item2);
        }

        [Test]
        public void TempTest()
        {
            object obj = "66".FromJson<int>();
            Assert.AreEqual(66, obj);

            obj = "\"true\"".FromJson<bool>();
            Assert.AreEqual(true, obj);

            obj = "\"null\"".FromJson<string>();
            Assert.AreEqual("null", obj);

            obj = "null".FromJson<string>();
            Assert.AreEqual(null, obj);

            obj = "\"null\"".FromJson<string>();
            Assert.AreEqual("null", obj);

            obj = "55.266e2".FromJson<string>();
            Assert.AreEqual("5526.6", obj);

            obj = "<root><char>a</char></root>".FromXml<char>();
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
            var json = obj.ToJson();
            Debug.Log(json);

            var obj2 = json.FromJson(type);
            Assert.AreEqual(obj, obj2);
            var json2 = obj2.ToJson();
            Assert.AreEqual(json, json2);
        }

        private void TestBinary(Type type, object obj)
        {
            var bytes = obj.ToBytes();
            var hex = "hex " + BitConverter.ToString(bytes);
            Debug.Log(hex);

            var obj2 = bytes.FromBytes(type);
            Assert.AreEqual(obj, obj2);
            var bytes2 = obj2.ToBytes();
            Assert.AreEqual(bytes, bytes2);
        }

        private void TestBase64(Type type, object obj)
        {
            var base64 = obj.ToBase64();
            Debug.Log(base64);

            var obj2 = base64.FromBase64(type);
            Assert.AreEqual(obj, obj2);
            var base64_2 = obj2.ToBase64();
            Assert.AreEqual(base64, base64_2);
        }

        private void TestXml(Type type, object obj)
        {
            var xml = obj.ToXml();
            Debug.Log(xml);

            var obj2 = xml.FromXml(type);
            Assert.AreEqual(obj, obj2);
            var xml2 = obj2.ToXml();
            Assert.AreEqual(xml, xml2);
        }
    }
}
