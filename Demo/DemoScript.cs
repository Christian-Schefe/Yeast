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

            var json = testData.ToJson();
            Debug.Log("JSON: " + json);

            var xml = testData.ToXml();
            Debug.Log("XML: " + xml);

            var parsedDataJson = json.FromJson<MyData>();
            var parsedDataXml = xml.FromXml<MyData>();

            var json2 = parsedDataJson.ToJson();
            Debug.Log("JSON after roundtrip: " + json2);

            var xml2 = parsedDataXml.ToXml();
            Debug.Log("XML after roundtrip: " + xml2);

            if (!new Circular().TryToJson(out _))
            {
                Debug.Log("Failed to serialize data");
            }

            try
            {
                new Circular().ToJson();
            }
            catch (System.Exception e)
            {
                Debug.Log("Failed to serialize data: " + e.Message);
            }

            if (!"\"string\"".TryFromJson<int>(out _))
            {
                Debug.Log("Failed to parse data");
            }
        }
    }
}
