using System;
using System.Collections.Generic;

namespace lib
{
    public static class RandomExtensions
    {
        public static T SelectOne<T>(this Random r, IList<T> list)
        {
            return list[r.Next(list.Count)];
        }

        public static T[] Sample<T>(this Random r, IList<T> list, int sampleSize)
        {
            var sample = new T[sampleSize];
            for (var i = 0; i < sampleSize; i++)
            {
                sample[i] = list[r.Next(list.Count)];
            }

            return sample;
        }

        public static bool Chance(this Random r, double probability)
        {
            return r.NextDouble() < probability;
        }

        public static ulong NextUlong(this Random r)
        {
            var a = unchecked((ulong)r.Next());
            var b = unchecked((ulong)r.Next());
            return (a << 32) | b;
        }

        public static double NextDouble(this Random r, double min, double max)
        {
            return r.NextDouble() * (max - min) + min;
        }

        public static ulong[,,] CreateRandomTable(this Random r, int size1, int size2, int size3)
        {
            var res = new ulong[size1, size2, size3];
            for (var x = 0; x < size1; x++)
            for (var y = 0; y < size2; y++)
            for (var h = 0; h < size3; h++)
            {
                var value = r.NextUlong();
                res[x, y, h] = value;
            }

            return res;
        }

        public static ulong[,] CreateRandomTable(this Random r, int size1, int size2)
        {
            var res = new ulong[size1, size2];
            for (var x = 0; x < size1; x++)
            for (var y = 0; y < size2; y++)
            {
                var value = r.NextUlong();
                res[x, y] = value;
            }

            return res;
        }
    }
}
