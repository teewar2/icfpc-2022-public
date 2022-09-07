using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace lib
{
    public static class Extensions
    {
        public static int SetBit(this int x, int bitIndex)
        {
            return x | (1 << bitIndex);
        }

        public static int GetBit(this int x, int bitIndex)
        {
            return (x >> bitIndex) & 1;
        }

        public static double Distance(this double a, double b)
        {
            return Math.Abs(a - b);
        }

        public static double Squared(this double x)
        {
            return x * x;
        }

        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            var i = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return i;
                i++;
            }

            return -1;
        }

        public static T MinBy<T>(this IEnumerable<T> items, Func<T, IComparable> getKey)
        {
            var best = default(T);
            IComparable? bestKey = null;
            foreach (var item in items)
            {
                var itemKey = getKey(item);
                if (bestKey == null || itemKey.CompareTo(bestKey) < 0)
                {
                    best = item;
                    bestKey = getKey(best);
                }
            }

            return best ?? throw new InvalidOperationException("collection is empty");
        }

        [return:MaybeNull]
        public static T MinByOrDefault<T>(this IEnumerable<T> items, Func<T, IComparable> getKey, [AllowNull] T defaultValue = default)
        {
            var best = default(T);
            IComparable? bestKey = null;
            foreach (var item in items)
            {
                var itemKey = getKey(item);
                if (bestKey == null || itemKey.CompareTo(bestKey) < 0)
                {
                    best = item;
                    bestKey = getKey(best);
                }
            }

            return bestKey == null ? defaultValue : best;
        }

        public static double MaxOrDefault<T>(this IEnumerable<T> items, Func<T, double> getKey, double defaultValue)
        {
            var bestCost = double.NegativeInfinity;
            foreach (var item in items)
            {
                var cost = getKey(item);
                if (cost > bestCost)
                    bestCost = cost;
            }
            return double.IsNegativeInfinity(bestCost) ? defaultValue : bestCost;
        }

        public static T MaxBy<T>(this IEnumerable<T> items, Func<T, IComparable> getKey)
        {
            var best = default(T);
            IComparable? bestKey = null;
            foreach (var item in items)
            {
                var itemKey = getKey(item);
                if (bestKey == null || itemKey.CompareTo(bestKey) > 0)
                {
                    best = item;
                    bestKey = itemKey;
                }
            }

            return best ?? throw new InvalidOperationException("collection is empty");
        }

        /// <summary>
        /// Inclusive bounds
        /// </summary>
        public static int BoundTo(this int v, int left, int right)
        {
            if (v < left) return left;
            if (v > right) return right;
            return v;
        }

        public static double BoundTo(this double v, double left, double right)
        {
            if (v < left) return left;
            if (v > right) return right;
            return v;
        }

        public static double TruncateAbs(this double v, double maxAbs)
        {
            if (v < -maxAbs) return -maxAbs;
            if (v > maxAbs) return maxAbs;
            return v;
        }

        public static int TruncateAbs(this int v, int maxAbs)
        {
            if (v < -maxAbs) return -maxAbs;
            if (v > maxAbs) return maxAbs;
            return v;
        }

        public static IEnumerable<T> Times<T>(this int count, Func<int, T> create)
        {
            return Enumerable.Range(0, count).Select(create);
        }

        public static IEnumerable<T> Times<T>(this int count, T item)
        {
            return Enumerable.Repeat(item, count);
        }

        /// <summary>
        /// Inclusive bounds
        /// </summary>
        public static bool InRange(this int v, int min, int max)
        {
            return v >= min && v <= max;
        }

        /// <summary>
        /// Inclusive bounds
        /// </summary>
        public static bool InRange(this double v, double min, double max)
        {
            return v >= min && v <= max;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> readOnlyList, T value)
        {
            var count = readOnlyList.Count;
            var equalityComparer = EqualityComparer<T>.Default;
            for (var i = 0; i < count; i++)
            {
                var current = readOnlyList[i];
                if (equalityComparer.Equals(current, value)) return i;
            }

            return -1;
        }

        public static TV GetOrCreate<TK, TV>(this IDictionary<TK, TV> d, TK key, Func<TK, TV> create)
        {
            return d.TryGetValue(key, out var v)
                ? v
                : d[key] = create(key);
        }

        public static TV GetOrDefault<TK, TV>(this IDictionary<TK, TV> d, TK key, TV def)
        {
            return d.TryGetValue(key, out var v) ? v : def;
        }

        public static int ElementwiseHashcode<T>(this IEnumerable<T> items)
        {
            unchecked
            {
                return items.Select(t => t?.GetHashCode() ?? 0).Aggregate((res, next) => (res * 379) ^ next);
            }
        }

        public static List<T> Shuffle<T>(this IEnumerable<T> items, Random random)
        {
            var copy = items.ToList();
            for (var i = 0; i < copy.Count; i++)
            {
                var nextIndex = random.Next(i, copy.Count);
                (copy[nextIndex], copy[i]) = (copy[i], copy[nextIndex]);
            }

            return copy;
        }

        public static T Sample<T>(this IList<T> items, Random random)
        {
            var index = random.Next(items.Count);
            return items[index];
        }

        public static T Sample<T>(this ICollection<T> items, Random random)
        {
            var index = random.Next(items.Count);
            return items.ElementAt(index);
        }

        /// <returns>angle in (-pi..pi]</returns>
        public static double NormAngleInRadians(this double angle)
        {
            while (angle > Math.PI) angle -= 2 * Math.PI;
            while (angle <= -Math.PI) angle += 2 * Math.PI;
            return angle;
        }

        public static double NormDistance(this double value, double worldDiameter)
        {
            return value / worldDiameter;
        }

        public static int ToInt(this string s)
        {
            return int.Parse(s);
        }
        public static long ToLong(this string s)
        {
            return long.Parse(s);
        }

        public static string StrJoin<T>(this IEnumerable<T> items, string delimiter)
        {
            return string.Join(delimiter, items);
        }

        public static string StrJoin<T>(this IEnumerable<T> items, char delimiter)
        {
            return string.Join(delimiter, items);
        }

        public static string StrJoin<T>(this IEnumerable<T> items, string delimiter, Func<T, string> toString)
        {
            return items.Select(toString).StrJoin(delimiter);
        }

        public static bool IsOneOf<T>(this T item, params T[] set)
        {
            return set.IndexOf(item) >= 0;
        }
        public static string ToCompactString(this double x)
        {
            if (Math.Abs(x) > 100) return x.ToString("0", CultureInfo.InvariantCulture);
            if (Math.Abs(x) > 10) return x.ToString("0.#", CultureInfo.InvariantCulture);
            if (Math.Abs(x) > 1) return x.ToString("0.##", CultureInfo.InvariantCulture);
            if (Math.Abs(x) > 0.1) return x.ToString("0.###", CultureInfo.InvariantCulture);
            if (Math.Abs(x) > 0.01) return x.ToString("0.####", CultureInfo.InvariantCulture);
            return x.ToString(CultureInfo.InvariantCulture);
        }

    }

}
