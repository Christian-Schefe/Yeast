using System;
using Yeast.Memento;

namespace Yeast.Utils
{
    public static class ArrayUtils
    {
        public static IMemento ArrayToMemento(Func<object, IMemento> transformer, Array arr)
        {
            int rank = arr.Rank;
            return ArrayToMementoRecursive(transformer, arr, rank, 0, new int[rank]);
        }

        private static IMemento ArrayToMementoRecursive(Func<object, IMemento> transformer, Array arr, int rank, int dimension, int[] indices)
        {
            int length = arr.GetLength(dimension);
            IMemento[] ionValueArr = new IMemento[length];

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

                    IMemento subArray = ArrayToMementoRecursive(transformer, arr, rank, dimension + 1, indices);
                    ionValueArr[i] = subArray;
                }
            }
            return new ArrayMemento(ionValueArr);
        }

        public static Array MementoToArray(Type elementType, Type arrayType, Func<IMemento, object> transformer, ArrayMemento ionValue)
        {
            int rank = arrayType.GetArrayRank();

            int[] lengths = new int[rank];
            GetLengthsInMemento(ionValue, rank, lengths, 0);

            Array arr = Array.CreateInstance(elementType, lengths);
            FillMultDimArrayRecursive(transformer, arr, ionValue, rank, 0, new int[rank]);
            return arr;
        }

        private static void GetLengthsInMemento(ArrayMemento ionValue, int rank, int[] lengths, int dimension)
        {
            lengths[dimension] = ionValue.value.Length;
            if (dimension < rank - 1 && ionValue.value.Length > 0)
            {
                GetLengthsInMemento((ArrayMemento)ionValue.value[0], rank, lengths, dimension + 1);
            }
        }

        private static void FillMultDimArrayRecursive(Func<IMemento, object> transformer, Array arr, ArrayMemento ionValue, int rank, int dimension, int[] indices)
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
                    FillMultDimArrayRecursive(transformer, arr, (ArrayMemento)ionValue.value[i], rank, dimension + 1, indices);
                }
            }
        }
    }
}
