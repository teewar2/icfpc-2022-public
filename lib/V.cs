using System;
using System.Collections.Generic;
using System.Globalization;
using MongoDB.Bson.Serialization.Attributes;

namespace lib
{
    public class V : IEquatable<V>
    {
        public static readonly V Zero = new V(0, 0);
        public static readonly V Up = new V(0, -1);
        public static readonly V Down = new V(0, 1);
        public static readonly V Left = new V(-1, 0);
        public static readonly V Right = new V(1, 0);

        public static readonly V[] Directions2 = { new V(1, 0), new V(0, 1) };
        public static readonly V[] Directions4 = { new V(1, 0), new V(0, 1), new V(-1, 0), new V(0, -1) };
        public static readonly V[] Directions5 = { Zero, new V(1, 0), new V(0, 1), new V(-1, 0), new V(0, -1) };
        public static readonly V[] Directions8 = {
            new V(-1, -1), new V(0, -1), new V(1, -1),
            new V(-1, 0), new V(0, 0), new V(1, 0),
            new V(-1, 1), new V(0, 1), new V(1, 1),
        };

        public static readonly V[] Directions9 = {
            Zero,
            new V(-1, -1), new V(0, -1), new V(1, -1),
            new V(-1, 0), new V(0, 0), new V(1, 0),
            new V(-1, 1), new V(0, 1), new V(1, 1),
        };

        [BsonElement]
        public readonly int X;

        [BsonElement]
        public readonly int Y;

        [BsonConstructor]
        public V(int x, int y)
        {
            X = x;
            Y = y;
        }

        public V(double x, double y)
            : this((int)Math.Round(x), (int)Math.Round(y))
        {
        }

        public double LenAsDouble => Math.Sqrt(Len2);
        public long Len2 => (long)X * X + (long)Y * Y;

        public static V Parse(string s)
        {
            var parts = s.Split(' ');
            return new V(int.Parse(parts[0]), int.Parse(parts[1]));
        }

        public bool Equals(V? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((V)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"[{X.ToString(CultureInfo.InvariantCulture)},{Y.ToString(CultureInfo.InvariantCulture)}]";
        }

        public long Dist2To(V point) => (this - point).Len2;

        public double DistTo(V b) => Math.Sqrt(Dist2To(b));

        public static bool operator==(V left, V right) => Equals(left, right);
        public static bool operator!=(V left, V right) => !Equals(left, right);

        public static implicit operator V(string s)
        {
            var parts = s.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var x = int.Parse(parts[0], CultureInfo.InvariantCulture);
            var y = int.Parse(parts[1], CultureInfo.InvariantCulture);
            return new V(x, y);
        }

        public static V operator+(V a, V b) => new(a.X + b.X, a.Y + b.Y);
        public static V operator-(V a, V b) => new(a.X - b.X, a.Y - b.Y);
        public static V operator-(V a) => new(-a.X, -a.Y);
        public static V operator*(V a, int k) => new(k * a.X, k * a.Y);
        public static V operator*(int k, V a) => new(k * a.X, k * a.Y);
        public static V operator*(double k, V a) => new(k * a.X, k * a.Y);
        public static V operator/(V a, int k) => new(a.X / k, a.Y / k);
        public static long operator*(V a, V b) => a.X * b.X + a.Y * b.Y;
        public static long operator^(V a, V b) => a.X * b.Y - a.Y * b.X;

        public int ScalarProd(V b)
        {
            return X * b.X + Y * b.Y;
        }

        public V[] GetNear8()
        {
            return new[]
            {
                new V(X-1, Y-1),
                new V(X-1, Y),
                new V(X-1, Y+1),
                new V(X, Y-1),
                new V(X, Y+1),
                new V(X+1, Y-1),
                new V(X+1, Y),
                new V(X+1, Y+1)
            };
        }
        public V[] GetNear9()
        {
            return new[]
            {
                new V(X-1, Y-1),
                new V(X-1, Y),
                new V(X-1, Y+1),
                new V(X, Y-1),
                new V(X, Y),
                new V(X, Y+1),
                new V(X+1, Y-1),
                new V(X+1, Y),
                new V(X+1, Y+1)
            };
        }

        public double GetAngleTo(V v)
        {
            var cos = ScalarProd(v) / LenAsDouble / v.LenAsDouble;
            return Math.Acos(cos);
        }
        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public static IEnumerable<V> AllInRange(int width, int height)
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                yield return new V(x, y);
            }
        }

        public int MDistTo(V v2)
        {
            var (x, y) = v2;
            return Math.Abs(x - X) + Math.Abs(y - Y);
        }

        public int MLen => Math.Abs(X) + Math.Abs(Y);

        public int CDistTo(V v2)
        {
            var (x, y) = v2;
            return Math.Max(Math.Abs(x - X), Math.Abs(y - Y));
        }

        public int CLen => Math.Max(Math.Abs(X), Math.Abs(Y));

        public bool InRange(int width, int height)
        {
            return X >= 0 && X < width && Y >= 0 && Y < height;
        }

        public bool IsStrictlyInside(Block block)
        {
            return IsStrictlyInside(block.BottomLeft, block.TopRight);
        }

        public bool IsStrictlyInside(V bottomLeft, V topRight)
        {
            return bottomLeft.X < X && X < topRight.X && bottomLeft.Y < Y && Y < topRight.Y;
        }
        public bool IsInside(V bottomLeft, V topRight)
        {
            return IsStrictlyInside(bottomLeft, topRight) || IsOnBoundary(bottomLeft, topRight);
        }

        public bool IsOnBoundary(V bottomLeft, V topRight)
        {
            return (bottomLeft.X == X && bottomLeft.Y <= Y && Y <= topRight.Y)
                   || (topRight.X == X && bottomLeft.Y <= Y && Y <= topRight.Y)
                   || (bottomLeft.Y == Y && bottomLeft.X <= X && X <= topRight.X)
                   || (topRight.Y == Y && bottomLeft.X <= X && X <= topRight.X);
        }

        public int GetScalarSize() => X * Y;

        public IEnumerable<V> Area9()
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                yield return new V(X + dx, Y + dy);
        }

        public IEnumerable<V> Area8()
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if (dx != 0 || dy != 0)
                    yield return new V(X + dx, Y + dy);
        }

        public IEnumerable<V> Area4()
        {
            yield return new V(X - 1, Y);
            yield return new V(X + 1, Y);
            yield return new V(X, Y - 1);
            yield return new V(X, Y + 1);
        }

        public IEnumerable<V> Area5()
        {
            yield return this;
            yield return new V(X - 1, Y);
            yield return new V(X + 1, Y);
            yield return new V(X, Y - 1);
            yield return new V(X, Y + 1);
        }

    }
}
