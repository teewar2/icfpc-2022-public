using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms
{
    public static class GeometricMedian
    {
        private static int[] Dr = new int[] { -1, 1, 0, 0, 0, 0, 0, 0 };
        private static int[] Dg = new int[] { 0, 0, -1, 1, 0, 0, 0, 0 };
        private static int[] Db = new int[] { 0, 0, 0, 0, -1, 1, 0, 0 };
        private static int[] Da = new int[] { 0, 0, 0, 0, 0, 0, -1, 1 };

        public static Rgba GetGeometricMedian(Screen screen, Block block)
        {
            var left = block.BottomLeft.X;
            var right = block.TopRight.X;
            var bottom = block.BottomLeft.Y;
            var top = block.TopRight.Y;
            return GetGeometricMedian(screen, left, right, bottom, top);
        }

        public static Rgba GetGeometricMedian(Screen screen, int left, int right, int bottom, int top)
        {
            var pixels = new List<Rgba>();
            for (int x = left; x < right; x++)
            {
                for (int y = bottom; y < top; y++)
                {
                    if (x >= screen.Pixels.GetLength(0)) continue;
                    if (y >= screen.Pixels.GetLength(1)) continue;
                    var pixel = screen.Pixels[x, y];
                    pixels.Add(pixel);
                }
            }

            return GetGeometricMedian(pixels.ToArray());
        }

        public static Rgba GetGeometricMedian(IList<Rgba> points, double eps = 1e-4)
        {
            var (rm, gm, bm, am) = (0.0, 0.0, 0.0, 0.0);
            foreach (var p in points)
            {
                rm += p.R;
                gm += p.G;
                bm += p.B;
                am += p.A;
            }
            rm /= points.Count;
            gm /= points.Count;
            bm /= points.Count;
            am /= points.Count;

            var d = EuclidDistance(rm, gm, bm, am, points);

            var step = 128.0;
            while (step > eps)
            {
                var isDone = false;
                for (var i = 0; i < 8; i++)
                {
                    var nr = rm + step * Dr[i];
                    var ng = gm + step * Dg[i];
                    var nb = bm + step * Db[i];
                    var na = am + step * Da[i];

                    var t = EuclidDistance(nr, ng, nb, na, points);

                    if (t < d)
                    {
                        d = t;
                        rm = nr;
                        gm = ng;
                        bm = nb;
                        am = na;

                        isDone = true;
                        break;
                    }
                }

                if (!isDone)
                    step /= 2;
            }

            return new Rgba((int) Math.Round(rm), (int) Math.Round(gm), (int) Math.Round(bm), (int) Math.Round(am));
        }

        private static double EuclidDistance(double r, double g, double b, double a, IList<Rgba> points)
        {
            var distance = 0.0;
            foreach (var other in points)
            {
                var rDist = (r - other.R) * (r - other.R);
                var gDist = (g - other.G) * (g - other.G);
                var bDist = (b - other.B) * (b - other.B);
                var aDist = (a - other.A) * (a - other.A);
                distance += Math.Sqrt(rDist + gDist + bDist + aDist);
            }
            return distance;
        }

        private static IEnumerable<IList<T>> Product<T>(IEnumerable<T> source, int repeat = 1)
        {
            var result = new List<List<T>> { new List<T>() };
            foreach (var pool in Enumerable.Repeat(source, repeat))
            {
                var newResult = new List<List<T>>();
                foreach (var r in result)
                    foreach (var x in pool)
                    {
                        newResult.Add(r.Append(x).ToList());
                    }
                result = newResult;
            }
            foreach (var prod in result)
                yield return prod.ToList();
        }
    }
}
