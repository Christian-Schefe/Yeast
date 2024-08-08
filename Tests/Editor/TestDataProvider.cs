using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yeast.Test
{
    public static class TestDataProvider
    {
        public static List<int> GetIntData()
        {
            return new List<int> { 0, 1, -1, 1234567, -1234567, int.MaxValue, int.MinValue };
        }

        public static List<float> GetFloatData()
        {
            return new List<float> { 0f, 1f, -1f, 1234567f, -1234567f, Mathf.PI, -Mathf.PI, 1e20f, -1e20f, 1234567.89f, -1234567.89f, float.Epsilon, float.MaxValue, float.MinValue, float.PositiveInfinity, float.NegativeInfinity, float.NaN };
        }

        public static List<double> GetDoubleData()
        {
            return new List<double> { 0d, 1d, -1d, 1234567d, -1234567d, System.Math.PI, -System.Math.PI, 1e20d, -1e20d, 1234567.89d, -1234567.89d, double.Epsilon, double.MaxValue, double.MinValue, double.PositiveInfinity, double.NegativeInfinity, double.NaN };
        }

        public static List<string> GetStringData()
        {
            return new List<string> { null, "", " ", "Hello, World!", "This is a multiline\nstring!", "This\nString\tcontains\0\r\a\b\vmany special characters\u0010 like #'+~*-_.:,;µ<>|@€!\"§$%&/()=?`´^°ê&amp;&&gt;" };
        }

        public static List<bool> GetBoolData()
        {
            return new List<bool> { true, false };
        }

        public static List<int[]> GetSimpleArrayData()
        {
            return new List<int[]>
            {
                new int[] { 0, 1, -1, 1234567, -1234567, int.MaxValue, int.MinValue },
                new int[] { },
                new int[] { 0 },
                new int[] { 0, 1 },
                null,
            };
        }

        public static List<int[,,]> GetMultidimensionalArrayData()
        {
            return new List<int[,,]>
            {
                new int[,,] { },
                new int[,,] { { } },
                new int[,,] { { { } } },
                new int[,,] { { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 10, 11, 12 } }, { { -1, -2, -3 }, { -4, -5, -6 }, { -7, -8, -9 }, { -10, -11, -12 } } },
                null,
            };
        }

        public static List<int[,,][,]> GetJaggedMultidimensionalArrayData()
        {
            int[,] i1 = new int[,] { { 0, 3 }, { 1, 3 }, { -2, -3 } };
            int[,] i2 = new int[,] { { 5, 7 }, { 4, 7 }, { 3, -7 } };

            return new List<int[,,][,]>
            {
                new int[,,][,] { },
                new int[,,][,] { { } },
                new int[,,][,] { { { } } },
                new int[,,][,] { { { } }, { { } }, { { } } },
                new int[,,][,] { { { null, null } }, { { null, null } }, { { null, null } } },
                new int[,,][,] { { { i1, i2, i1 }, { i1, i2, i1 } }, { { i2, i2, i1 }, { i2, i2, i2 } }, { { i1, i2, i1 }, { i1, i2, i1 } }, { { i2, i2, i1 }, { i2, i2, i2 } }},
                null
            };
        }

        public static List<List<int>> GetListData()
        {
            return new List<List<int>>
            {
                new List<int> { 0, 1, -1, 1234567, -1234567, int.MaxValue, int.MinValue },
                new List<int> { },
                new List<int> { 0 },
                new List<int> { 0, 1 },
                new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                null,
            };
        }

        public static List<Dictionary<string, int>> GetDictionaryData()
        {
            return new List<Dictionary<string, int>>
            {
                new() { },
                new() { { "a", 1 }, { "b", 2 }, { "c", 3 } },
                null,
            };
        }

        public static List<HashSet<string>> GetHashSetData()
        {
            return new List<HashSet<string>>
            {
                new() { },
                new() { "a", "b", "c" },
                null,
            };
        }

        public static List<School> GetClassData()
        {
            List<Person> people = new() {
                new Student("Alice", 20, 1.7f, 315), new Student("Bob", 21, 1.8f, 642), new Student("Charlie", 22, 1.9f, 426),
                new Professor("John", 44, 1.66f, 47)
            };

            return new List<School>
            {
                new("School 1", new() { new Student("Alice", 20, 1.7f, 12135), new Student("Bob", 21, 1.8f ,42342), new Student("Charlie", 22, 1.9f, 73544) }, people),
                new("School 2", new() { new Student("Alice", 20, 1.7f, 12135), new Student("Bob", 21, 1.8f ,42342), new Student("Charlie", 22, 1.9f, 73544) }, null),
                null
            };
        }

        public static List<(int, int, string)> GetTupleData()
        {
            return new()
            {
                (4, 5, "hello"),
                (532, -55, "Hello World!"),
            };
        }
        public static List<Type> GetTypeData()
        {
            return new()
            {
                typeof(int),
                typeof(string),
                typeof(School),
                typeof(List<>),
                typeof(List<int>),
                typeof(Dictionary<int, (string, School)>)
            };
        }

        public static List<Vector2> GetVector2Data()
        {
            return new List<Vector2> { Vector2.zero, Vector2.one, new(1f, 2f), new(-1f, -2f), new(1e20f, 1e20f), new(-1e20f, -1e20f), new(1234567.89f, 1234567.89f), new(-1234567.89f, -1234567.89f) };
        }

        public static List<(Type, object)> GetVariousData()
        {

            return new()
            {
                (typeof(int), 0),
                (typeof(string), null),
                (typeof(Vector3Int), new Vector3Int(1, 2, 3)),
                (typeof(DateTime), new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                (typeof(DateTimeOffset), new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero)),
                (typeof(TimeSpan), new TimeSpan(1, 2, 3, 4, 5)),
                (typeof(Guid), Guid.NewGuid()),
                (typeof(char), 'ß'),
                (typeof(Type), typeof(School)),
                (typeof(int?), 5),
                (typeof(int?), null),
                (typeof(Dictionary<string,int?>), null),
                (typeof(Dictionary<string,int?>), new Dictionary<string,int?>()),
                (typeof(Dictionary<string,int?>), new Dictionary<string,int?>(){ { "hi",5 }, {"hello",null } }),
            };
        }
    }
}
