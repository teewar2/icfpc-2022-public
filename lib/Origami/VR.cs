using System;
using System.Diagnostics.Contracts;

namespace lib.Origami
{
	public struct VR : IEquatable<VR>
    {
        public static readonly VR Zero = new(0, 0);
        public readonly Rational X, Y;

		public VR(Rational x, Rational y)
		{
			X = x;
			Y = y;
		}
		public static VR Parse(string s)
		{
			var parts = s.Split(',');
			if (parts.Length != 2) throw new FormatException(s);
			return new VR(Rational.Parse(parts[0]), Rational.Parse(parts[1]));
		}
		#region value semantics

        public bool Equals(VR other) =>
            X.Equals(other.X) && Y.Equals(other.Y);

        public override bool Equals(object? obj) =>
            obj is VR other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(X, Y);

        public static bool operator==(VR left, VR right) =>
            left.Equals(right);

        public static bool operator!=(VR left, VR right) =>
            !left.Equals(right);

        public override string ToString()
		{
			return $"{X},{Y}";
		}
		#endregion
		public static implicit operator VR(string s)
		{
			return Parse(s);
		}
		public static implicit operator VR(V v)
		{
			return new VR(v.X, v.Y);
		}
		public static VR operator -(VR a, VR b)
		{
			return new VR(a.X - b.X, a.Y - b.Y);
		}
		public static VR operator -(VR a)
		{
			return new VR(-a.X, -a.Y);
		}

		public static VR operator +(VR a, VR b)
		{
			return new VR(a.X + b.X, a.Y + b.Y);
		}
		public static VR operator *(VR a, Rational k)
		{
			return new VR(a.X * k, a.Y * k);
		}
		public static VR operator /(VR a, Rational k)
		{
			return new VR(a.X / k, a.Y / k);
		}
		public static VR operator *(Rational k, VR a)
		{
			return new VR(a.X * k, a.Y * k);
		}
		public Rational ScalarProd(VR p2)
		{
			return X * p2.X + Y * p2.Y;
		}

		public Rational VectorProdLength(VR p2)
		{
			return X * p2.Y - p2.X * Y;
		}

		[Pure]
		public VR Move(Rational shiftX, Rational shiftY)
		{
			return new VR(X + shiftX, Y + shiftY);
		}

        public VR Round()
		{
			return new VR(X.ToLong(), Y.ToLong());
		}

		public double Length => Math.Sqrt(X * X + Y * Y);
		public Rational Length2 => X * X + Y * Y;

        public VR[] GetNear8()
        {
            return new[]
            {
                new VR(X-1, Y-1),
                new VR(X-1, Y),
                new VR(X-1, Y+1),
                new VR(X, Y-1),
                new VR(X, Y+1),
                new VR(X+1, Y-1),
                new VR(X+1, Y),
                new VR(X+1, Y+1)
            };
        }

        public bool IsInside(RPolygon polygon)
        {
            return this.GetPositionToPolygon(polygon) != PointToPolygonPositionType.Outside;
        }

        public Rational Dist2To(VR other)
        {
            return (this - other).Length2;
        }

        public double DistTo(VR other)
        {
            return (this - other).Length;
        }

        public V ToV()
        {
            return new V(X.ToInt(), Y.ToInt());
        }

        public VR Rotate90() => new VR(Y, -X);
    }
}
