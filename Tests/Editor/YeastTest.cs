using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;


namespace Yeast.Test
{
    public class YeastTest
    {
        [Test]
        public void RunTests()
        {

            foreach (var data in TestDataProvider.GetIntData()) Test(data);
            foreach (var data in TestDataProvider.GetFloatData()) Test(data);
            foreach (var data in TestDataProvider.GetDoubleData()) Test(data);
            foreach (var data in TestDataProvider.GetStringData()) Test(data);
            foreach (var data in TestDataProvider.GetBoolData()) Test(data);
            foreach (var data in TestDataProvider.GetSimpleArrayData()) Test(data);
            foreach (var data in TestDataProvider.GetMultidimensionalArrayData()) Test(data);
            foreach (var data in TestDataProvider.GetJaggedMultidimensionalArrayData()) Test(data);
            foreach (var data in TestDataProvider.GetListData()) Test(data);
            foreach (var data in TestDataProvider.GetVector2Data()) Test(data);
            foreach (var data in TestDataProvider.GetDictionaryData()) Test(data);
            foreach (var data in TestDataProvider.GetHashSetData()) Test(data);
            foreach (var data in TestDataProvider.GetClassData()) Test(data);
            foreach (var data in TestDataProvider.GetVariousData()) Test(data.Item1, data.Item2);

            Test(TestDataProvider.GetIntData());
            Test(TestDataProvider.GetFloatData());
            Test(TestDataProvider.GetDoubleData());
            Test(TestDataProvider.GetStringData());
            Test(TestDataProvider.GetBoolData());
            Test(TestDataProvider.GetSimpleArrayData());
            Test(TestDataProvider.GetMultidimensionalArrayData());
            Test(TestDataProvider.GetJaggedMultidimensionalArrayData());
            Test(TestDataProvider.GetListData());
            Test(TestDataProvider.GetVector2Data());
            Test(TestDataProvider.GetDictionaryData());
            Test(TestDataProvider.GetHashSetData());
            Test(TestDataProvider.GetClassData());
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
            Debug.Log(obj + "\n" + obj2);
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
