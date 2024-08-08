using System.Collections.Generic;
using UnityEngine;

namespace Yeast.Demo
{
    public class DemoScript : MonoBehaviour
    {
        private struct MyData
        {
            public string name;
            public int age;
            public float speed;
            public List<int> scores;
            public Dictionary<string, List<int>> lists;
        }

        private class Circular
        {
            public Circular child;

            public Circular() { child = this; }
        }

        private void Start()
        {
            var testData = new MyData
            {
                name = "John",
                age = 25,
                speed = 1.5f,
                scores = new List<int> { 10, 20, 30 },
                lists = new Dictionary<string, List<int>>
                {
                    { "A", new List<int> { 1, 2, 3 } },
                    { "B", new List<int> { 4, 5 } }
                }
            };

            var json = Yeast.ToJson(testData);
            Debug.Log("JSON: " + json);

            var xml = Yeast.ToXml(testData);
            Debug.Log("XML: " + xml);

            var parsedDataJson = Yeast.FromJson<MyData>(json);
            var parsedDataXml = Yeast.FromXml<MyData>(xml);

            var json2 = Yeast.ToJson(parsedDataJson);
            Debug.Log("JSON after roundtrip: " + json2);

            var xml2 = Yeast.ToXml(parsedDataXml);
            Debug.Log("XML after roundtrip: " + xml2);

            if (!Yeast.TryToJson(new Circular(), out _))
            {
                Debug.Log("Failed to serialize data");
            }

            try
            {
                Yeast.ToJson(new Circular());
            }
            catch (System.Exception e)
            {
                Debug.Log("Failed to serialize data: " + e.Message);
            }

            if (!Yeast.TryFromJson<int>("\"string\"", out _))
            {
                Debug.Log("Failed to parse data");
            }
        }
    }
}
