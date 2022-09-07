using System.Linq;

namespace lib.Origami
{
	public static class VectorExtensions
	{
		public static VR Reflect(this VR p, VR a, VR b)
		{
			return p.Reflect(new RSeg(a, b));
		}
		public static VR Reflect(this VR p, RSeg mirror)
		{
			var b = mirror.End - mirror.Start;
			var a = p - mirror.Start;
			var k = a.ScalarProd(b)*2/b.Length2;
			return mirror.Start + b*k-a;
		}

        public static VR Reflect(this VR p, VR origin)
		{
            return origin - (p - origin);
		}

        public static VR HorizontalReflect(this VR p, VR origin)
		{
            return p.Reflect(origin, origin + new VR(1, 0));
		}

        public static VR VerticalReflect(this VR p, VR origin)
		{
            return p.Reflect(origin, origin + new VR(0, 1));
		}

		public static VR[] ToPoints(this string points)
		{
			return points.Split(' ').Select(VR.Parse).ToArray();
		}

		public static VR GetCenter(this VR[] ps)
		{
			var minX = ps.Select(v => v.X).Min();
			var minY = ps.Select(v => v.Y).Min();
			var maxX = ps.Select(v => v.X).Max();
			var maxY = ps.Select(v => v.Y).Max();
			return new VR((minX + maxX) / 2, (minY + maxY) / 2);
		}
		public static VR[] Rotate(this VR[] ps, Rational x)
		{
			return ps.Select(p => p.Rotate(ps.GetCenter(), x)).ToArray();
		}
		public static VR[] Move(this VR[] ps, VR shift)
		{
			return ps.Select(p => p + shift).ToArray();
		}
		public static VR Rotate(this VR p, VR other, Rational x)
		{
			return (p - other).Rotate(x) + other;
		}
		public static VR Rotate(this VR p, Rational x)
		{
			RSeg s1 = new RSeg(new VR(0, 0), new VR(new Rational(1, 2), new Rational(1, 2)));
			RSeg s2 = x < 0
				? new RSeg(new VR(0, 0), s1.End + new VR(x, 0))
				: new RSeg(new VR(0, 0), s1.End - new VR(0, x));
			return p.Reflect(s1).Reflect(s2);
		}
	}
}
