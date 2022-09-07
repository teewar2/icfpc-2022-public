using System;

namespace lib.Algorithms;

public static class MinMatchFinder
{
    public static int[] FindMinMatch(double [,] w)
    {
        var minValue = double.PositiveInfinity;
        for (int i = 0; i < w.GetLength(0); i++)
        for (int j = 0; j < w.GetLength(1); j++)
        {
            if (w[i, j] < minValue)
                minValue = w[i, j];
        }
        for (int i = 0; i < w.GetLength(0); i++)
        for (int j = 0; j < w.GetLength(1); j++)
            w[i, j] -= minValue;


        var n = w.GetLength(0);
        var m = w.GetLength(0);
        var u = new double [n + 1];
        var v = new double [m + 1];
        var p = new int [m + 1];
        var way = new int [m + 1];
        for (int i=1; i<=n; ++i)
        {
            p[0] = i;
            int j0 = 0;
            var minv = new double[m + 1];
            Array.Fill(minv, double.MaxValue);
            var used = new bool[m + 1];
            Array.Fill(used, false);
            do
            {
                used[j0] = true;
                int i0 = p[j0];
                var delta = double.MaxValue;
                int j1 = 0;
                for (int j=1; j<=m; ++j)
                    if (!used[j]) {
                        double cur = w[i0 - 1,j - 1]-u[i0]-v[j];
                        if (cur < minv[j])
                        {
                            minv[j] = cur;
                            way[j] = j0;
                        }
                        if (minv[j] < delta)
                        {
                            delta = minv[j];
                            j1 = j;
                        }
                    }
                for (int j=0; j<=m; ++j)
                    if (used[j])
                    {
                        u[p[j]] += delta;
                        v[j] -= delta;
                    }
                    else
                        minv[j] -= delta;
                j0 = j1;
            } while (p[j0] != 0);
            do {
                int j1 = way[j0];
                p[j0] = p[j1];
                j0 = j1;
            } while (j0 != 0);
        }
        var ans = new int[n];
        for (int j=1; j<=m; ++j)
            ans[p[j] - 1] = j - 1;
        return ans;
    }
}
