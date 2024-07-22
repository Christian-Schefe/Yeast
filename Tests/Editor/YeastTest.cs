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
        }

        private void Test<T>(T obj)
        {
            TestJson(obj);
            TestBinary(obj);
            TestBase64(obj);
            TestXml(obj);
        }

        private void TestJson<T>(T obj)
        {
            var json = Yeast.ToJson(obj);
            Debug.Log(json);

            var obj2 = Yeast.FromJson<T>(json);
            Assert.AreEqual(obj, obj2);
            var json2 = Yeast.ToJson(obj2);
            Assert.AreEqual(json, json2);
        }

        private void TestBinary<T>(T obj)
        {
            var bytes = Yeast.ToBytes(obj);
            var hex = "hex " + System.BitConverter.ToString(bytes);
            Debug.Log(hex);

            var obj2 = Yeast.FromBytes<T>(bytes);
            Assert.AreEqual(obj, obj2);
            var bytes2 = Yeast.ToBytes(obj2);
            Assert.AreEqual(bytes, bytes2);
        }

        private void TestBase64<T>(T obj)
        {
            var base64 = Yeast.ToBase64(obj);
            Debug.Log(base64);

            var obj2 = Yeast.FromBase64<T>(base64);
            Assert.AreEqual(obj, obj2);
            var base64_2 = Yeast.ToBase64(obj2);
            Assert.AreEqual(base64, base64_2);
        }

        private void TestXml<T>(T obj)
        {
            var xml = Yeast.ToXml(obj);
            Debug.Log(xml);

            var obj2 = Yeast.FromXml<T>(xml);
            Assert.AreEqual(obj, obj2);
            var xml2 = Yeast.ToXml(obj2);
            Assert.AreEqual(xml, xml2);
        }
    }
}
