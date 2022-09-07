using System;
using System.Numerics;

namespace lib.Origami
{
    public static class Arithmetic
    {
        /// <summary>
        /// Возвращает наибольшее число, меньше или равное корню из n
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static BigInteger Sqrt(BigInteger n)
        {
            if (n == BigInteger.Zero) return BigInteger.Zero;
            var left = BigInteger.One;
            var right = n;
            while (right - left > 1)
            {
                var m = (right + left) / 2;
                var t = m * m;
                if (t <= n) left = m;
                else right = m;
            }

            return left;
        }

        public static Rational Sqrt(Rational r)
        {
            r = r.Reduce();
            return new Rational(Sqrt(r.Numerator), Sqrt(r.Denomerator));
        }

        public static bool IsSquare(BigInteger n)
        {
            var sq = Sqrt(n);
            return n == sq * sq;
        }

        public static bool IsSquare(Rational r)
        {
            r = r.Reduce();
            return IsSquare(r.Numerator) && IsSquare(r.Denomerator);
        }

        public static double IrrationalDistance(VR a, VR b)
        {
            var dx = (double)(a.X - b.X);
            var dy = (double)(a.Y - b.Y);
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static bool PointInSegment(VR a, RSeg b)
        {
            if ((a - b.Start).VectorProdLength(b.End - b.Start) != 0) return false;
            if ((b.End - b.Start).ScalarProd(a - b.Start) < 0) return false;
            if ((b.Start - b.End).ScalarProd(a - b.End) < 0) return false;
            return true;
        }

        public static VR? GetIntersection(this RSeg segment, RSeg intersector)
        {
            var A1 = segment.End - segment.Start;
            var B1 = segment.Start;

            var A2 = intersector.End - intersector.Start;
            var B2 = intersector.Start;

            var denominator = A1.Y * A2.X - A2.Y * A1.X;

            if (denominator == 0)
                return null;

            var t2 = ((B2.Y - B1.Y) * A1.X + (B1.X - B2.X) * A1.Y) / denominator;

            var point = A2 * t2 + B2;

            if (IsBetween(segment.Start.X, point.X, segment.End.X) && IsBetween(segment.Start.Y, point.Y, segment.End.Y) && IsBetween(intersector.Start.X, point.X, intersector.End.X) && IsBetween(intersector.Start.Y, point.Y, intersector.End.Y))
                return new VR(point.X.Reduce(), point.Y.Reduce());

            return null;
        }

        public static V[] Triangulate(V a, V b, Rational axLen2, Rational bxLen2)
        {
            var abLen2 = (b - a).Len2;
            if (a == b)
            {
                throw new ArgumentException();
            }
            if (Math.Sqrt(axLen2) + Math.Sqrt(bxLen2) < Math.Sqrt(abLen2))
                return new[]
                {
                    a + Math.Sqrt(axLen2) / Math.Sqrt(abLen2) * (b - a),
                    b + Math.Sqrt(bxLen2) / Math.Sqrt(abLen2) * (a - b),
                };
            if (Math.Sqrt(axLen2) + Math.Sqrt(abLen2) < Math.Sqrt(bxLen2))
            {
                return new[]
                {
                    a - Math.Sqrt(axLen2) / Math.Sqrt(abLen2) * (b - a),
                    b + Math.Sqrt(bxLen2) / Math.Sqrt(abLen2) * (a - b),
                };
            }
            if (Math.Sqrt(bxLen2) + Math.Sqrt(abLen2) < Math.Sqrt(axLen2))
            {
                return new[]
                {
                    a + Math.Sqrt(axLen2) / Math.Sqrt(abLen2) * (b - a),
                    b - Math.Sqrt(bxLen2) / Math.Sqrt(abLen2) * (a - b),
                };
            }

            var bhNumerator = abLen2 - axLen2 + bxLen2;
            var relativeBhLength = bhNumerator / (2 * abLen2);
            V eba = a - b;
            var hx = b.X + relativeBhLength * eba.X;
            var hy = b.Y + relativeBhLength * eba.Y;
            V oba = new(-eba.Y, eba.X);
            var bh2 = bhNumerator * bhNumerator / (4 * abLen2);
            var multiplier2 = (bxLen2 - bh2) / abLen2;

            var multiplier = Math.Sqrt(multiplier2);
            var first = new V(hx + multiplier*oba.X, hy + multiplier*oba.Y);
            var second = new V(hx - multiplier * oba.X, hy - multiplier * oba.Y);
            return new[] {first, second};
        }

        public static VR[]? RationalTriangulate(RSeg ax, RSeg bx, VR a, VR b)
        {
            var ab = new RSeg(a, b);
            if (ab.Length2 == 0) return null;
            var bh_numerator = ab.Length2 - ax.Length2 + bx.Length2;
            var relative_bh_length = bh_numerator / (2 * ab.Length2);
            var eba = a - b;
            var h = b + eba * relative_bh_length;
            var oba = new VR(-eba.Y, eba.X);
            var bh2 = bh_numerator * bh_numerator / (4 * ab.Length2);
            var multiplier2 = (bx.Length2 - bh2) / ab.Length2;
            if (!IsSquare(multiplier2)) return null;

            var multiplier = Sqrt(multiplier2);
            var first = h + oba * multiplier;
            var second = h - oba * multiplier;
            return new[] {first, second};
        }

        public static Rational Distance2(VR a, VR b) => (b - a).Length2;

        public static Rational Distance2(VR point, RSeg segment)
        {
            var v = segment.End - segment.Start;
            var w = point - segment.Start;
            var c1 = w.ScalarProd(v);
            var c2 = v.ScalarProd(v);
            if (c1 <= 0)
                return Distance2(point, segment.Start);
            if (c2 <= c1)
                return Distance2(point, segment.End);
            var b = c1 / c2;
            var pb = segment.Start + b * v;
            return Distance2(point, pb);
        }

        public static Rational? InDistance2(VR point, RSeg segment)
        {
            var v = segment.End - segment.Start;
            var w = point - segment.Start;
            var c1 = w.ScalarProd(v);
            var c2 = v.ScalarProd(v);
            var b = c1 / c2;
            var pb = segment.Start + b * v;
            return Distance2(point, pb);
        }

        private static bool IsBetween(Rational a, Rational x, Rational b) =>
            (a - x) * (b - x) <= 0;
    }
}
