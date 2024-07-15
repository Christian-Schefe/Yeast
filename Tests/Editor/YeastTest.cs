using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Yeast.Binary;
using Yeast.Json;


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
            foreach (var data in TestDataProvider.GetListData()) Test(data);
            foreach (var data in TestDataProvider.GetVector2Data()) Test(data);
        }

        private void Test<T>(T obj)
        {
            TestJson(obj);
            TestBinary(obj);
        }

        private void TestJson<T>(T obj)
        {
            var json = JSON.Stringify(obj);
            Debug.Log(json);

            var obj2 = JSON.Parse<T>(json);
            Assert.AreEqual(obj, obj2);
            var json2 = JSON.Stringify(obj2);
            Assert.AreEqual(json, json2);
        }

        private void TestBinary<T>(T obj)
        {
            var bytes = BINARY.Serialize(obj);
            Debug.Log(bytes);

            var obj2 = BINARY.Deserialize<T>(bytes);
            Assert.AreEqual(obj, obj2);
            var bytes2 = BINARY.Serialize(obj2);
            Assert.AreEqual(bytes, bytes2);
        }
    }
}
