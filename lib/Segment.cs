using static System.Math;

namespace lib
{
    public class Segment
    {
        public Segment(V v1, V v2)
        {
            V1 = v1;
            V2 = v2;
        }

        public V V1 { get; }
        public V V2 { get; }

        public long Length2 => V1.Dist2To(V2);

        public bool IntersectsWith(Segment other)
        {
            var a1 = V2.Y - V1.Y;
            var b1 = V1.X - V2.X;
            var c1 = a1 * V1.X + b1 * V1.Y;

            var a2 = other.V2.Y - other.V1.Y;
            var b2 = other.V1.X - other.V2.X;
            var c2 = a2 * other.V1.X + b2 * other.V1.Y;

            var delta = a1 * b2 - a2 * b1;
            if (delta == 0)
                return false;

            var vd = new V(b2 * c1 - b1 * c2, a1 * c2 - a2 * c1);
            var v1 = V1 * delta;
            var v2 = V2 * delta;
            if (v1.X == v2.X)
                return vd.Y > Min(v1.Y, v2.Y)
                       && vd.Y < Max(v1.Y, v2.Y);
            if (v1.Y == v2.Y)
                return vd.X > Min(v1.X, v2.X)
                       && vd.X < Max(v1.X, v2.X);

            return vd.X > Min(v1.X, v2.X)
                   && vd.X < Max(v1.X, v2.X)
                   && vd.Y > Min(v1.Y, v2.Y)
                   && vd.Y < Max(v1.Y, v2.Y);
        }
    }
}
