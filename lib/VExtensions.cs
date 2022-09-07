using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace lib
{
    public static class VExtensions
    {
        public static (V topleft, V bottomRight) GetBoundingBox(this IList<V> vs)
        {
            var minX = vs.Min(v => v.X);
            var minY = vs.Min(v => v.Y);
            var maxX = vs.Max(v => v.X);
            var maxY = vs.Max(v => v.Y);
            return (new V(minX, minY), new V(maxX, maxY));
        }

        static bool OnSegment(V p, V q, V r)
        {
            if (q.X <= Math.Max(p.X, r.X) &&
                q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) &&
                q.Y >= Math.Min(p.Y, r.Y))
            {
                return true;
            }

            return false;
        }

        // To find orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are colinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        static int Orientation(V p, V q, V r)
        {
            int val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (val == 0)
            {
                return 0; // colinear
            }

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        // The function that returns true if
        // line segment 'p1q1' and 'p2q2' intersect.
        static bool DoIntersect(
            V p1,
            V q1,
            V p2,
            V q2)
        {
            // Find the four orientations needed for
            // general and special cases
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            // Special Cases
            // p1, q1 and p2 are colinear and
            // p2 lies on segment p1q1
            if (o1 == 0 && OnSegment(p1, p2, q1))
            {
                return true;
            }

            // p1, q1 and p2 are colinear and
            // q2 lies on segment p1q1
            if (o2 == 0 && OnSegment(p1, q2, q1))
            {
                return true;
            }

            // p2, q2 and p1 are colinear and
            // p1 lies on segment p2q2
            if (o3 == 0 && OnSegment(p2, p1, q2))
            {
                return true;
            }

            // p2, q2 and q1 are colinear and
            // q1 lies on segment p2q2
            if (o4 == 0 && OnSegment(p2, q1, q2))
            {
                return true;
            }

            // Doesn't fall in any of the above cases
            return false;
        }

        // Returns true if the point p lies
        // inside the polygon[] with n vertices
        public static bool IsInside(this V p, V[] polygon)
        {
            // There must be at least 3 vertices in polygon[]
            if (polygon.Length < 3)
            {
                return false;
            }

            // Create a point for line segment from p to infinite
            V extreme = new(polygon.Select(x => x.X).Max() + 1, p.Y);

            // Count intersections of the above line
            // with sides of polygon
            int count = 0, i = 0;
            do
            {
                int next = (i + 1) % polygon.Length;

                // Check if the line segment from 'p' to
                // 'extreme' intersects with the line
                // segment from 'polygon[i]' to 'polygon[next]'
                if (DoIntersect(
                    polygon[i],
                    polygon[next],
                    p,
                    extreme))
                {
                    // If the point 'p' is colinear with line
                    // segment 'i-next', then check if it lies
                    // on segment. If it lies, return true, otherwise false
                    if (Orientation(polygon[i], p, polygon[next]) == 0)
                    {
                        return OnSegment(
                            polygon[i],
                            p,
                            polygon[next]);
                    }

                    count++;
                }

                i = next;
            } while (i != 0);

            // Return true if count is odd, false otherwise
            return (count % 2 == 1); // Same as (count%2 == 1)
        }

        public static V Reflect(this V p, V start, V finish)
        {
            var b = finish - start;
            var a = p - start;
            var k = (int) Math.Floor(a.ScalarProd(b) * 2 / (double) b.Len2);
            return start + b * k - a;
        }

        public static V Reflect(this V p, V origin)
        {
            return origin - (p - origin);
        }

        public static V HorizontalReflect(this V p, V a) => p.Reflect(a, a + new V(1, 0));

        public static V VerticalReflect(this V p, V a) => p.Reflect(a, a + new V(0, 1));

        /// <summary>
        /// Rotates vector around pivot clockwise
        /// </summary>
        public static V Rotate(this V p, V around, int angle)
        {
            var cos = Math.Cos(-angle * Math.PI / 180);
            var sin = Math.Sin(-angle * Math.PI / 180);
            var centered = p - around;
            var x = Math.Round(centered.X * cos - centered.Y * sin);
            var y = Math.Round(centered.X * sin + centered.Y * cos);
            return new V(x, y) + around;
        }

        public static V Rotate(this V p, V around, double angleInRadians)
        {
            var cos = Math.Cos(angleInRadians);
            var sin = Math.Sin(angleInRadians);
            var centered = p - around;
            var x = Math.Round(centered.X * cos - centered.Y * sin);
            var y = Math.Round(centered.X * sin + centered.Y * cos);
            return new V(x, y) + around;
        }
    }
}
