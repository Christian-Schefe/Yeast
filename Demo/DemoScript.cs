using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yeast.Json;

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

            var json = JSON.Stringify(testData);
            Debug.Log(json);

            var parsedData = JSON.Parse<MyData>(json);

            var json2 = JSON.Stringify(parsedData);
            Debug.Log(json2);
        }
    }
}
