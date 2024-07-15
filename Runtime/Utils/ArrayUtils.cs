using System;
using Yeast.Ion;

namespace Yeast.Utils
{
    public static class ArrayUtils
    {
        public static IIonValue ArrayToIonValue(Func<object, IIonValue> transformer, Array arr)
        {
            int rank = arr.Rank;
            return ArrayToIonValueRecursive(transformer, arr, rank, 0, new int[rank]);
        }

        private static IIonValue ArrayToIonValueRecursive(Func<object, IIonValue> transformer, Array arr, int rank, int dimension, int[] indices)
        {
            int length = arr.GetLength(dimension);
            IIonValue[] ionValueArr = new IIonValue[length];

            if (dimension == rank - 1)
            {
                for (int i = 0; i < length; i++)
                {
                    indices[dimension] = i;
                    ionValueArr[i] = transformer(arr.GetValue(indices));
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    indices[dimension] = i;

                    IIonValue subArray = ArrayToIonValueRecursive(transformer, arr, rank, dimension + 1, indices);
                    ionValueArr[i] = subArray;
                }
            }
            return new ArrayValue(ionValueArr);
        }

        public static Array IonValueToArray(Type elementType, Type arrayType, Func<IIonValue, object> transformer, ArrayValue ionValue)
        {
            int rank = arrayType.GetArrayRank();

            int[] lengths = new int[rank];
            GetLengthsInIonValue(ionValue, rank, lengths, 0);

            Array arr = Array.CreateInstance(elementType, lengths);
            FillMultDimArrayRecursive(transformer, arr, ionValue, rank, 0, new int[rank]);
            return arr;
        }

        private static void GetLengthsInIonValue(ArrayValue ionValue, int rank, int[] lengths, int dimension)
        {
            lengths[dimension] = ionValue.value.Length;
            if (dimension < rank - 1 && ionValue.value.Length > 0)
            {
                GetLengthsInIonValue((ArrayValue)ionValue.value[0], rank, lengths, dimension + 1);
            }
        }

        private static void FillMultDimArrayRecursive(Func<IIonValue, object> transformer, Array arr, ArrayValue ionValue, int rank, int dimension, int[] indices)
        {
            int length = arr.GetLength(dimension);

            if (dimension == rank - 1)
            {
                for (int i = 0; i < length; i++)
                {
                    indices[dimension] = i;
                    arr.SetValue(transformer(ionValue.value[i]), indices);
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    indices[dimension] = i;
                    FillMultDimArrayRecursive(transformer, arr, (ArrayValue)ionValue.value[i], rank, dimension + 1, indices);
                }
            }
        }
    }
}
