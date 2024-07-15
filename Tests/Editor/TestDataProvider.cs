using System.Collections;
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
            return new List<string> { "", " ", "Hello, World!", "This is a multiline\nstring!", "This\nString\tcontains\0\r\a\b\vmany special characters\u0010 like #'+~*-_.:,;µ<>|@€!\"§$%&/()=?`´^°ê" };
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
            };
        }

        public static List<Vector2> GetVector2Data()
        {
            return new List<Vector2> { Vector2.zero, Vector2.one, new(1f, 2f), new(-1f, -2f), new(1e20f, 1e20f), new(-1e20f, -1e20f), new(1234567.89f, 1234567.89f), new(-1234567.89f, -1234567.89f) };
        }
    }
}
