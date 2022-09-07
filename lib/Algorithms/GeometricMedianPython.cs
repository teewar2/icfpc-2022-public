using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace lib.Algorithms;

// public class GeometricMedianPython
// {
//     private readonly dynamic module;
//     public GeometricMedianPython()
//     {
//         Init().GetAwaiter().GetResult();
//
//         module = PythonEngine.ModuleFromString(string.Empty, @"
// import numpy as np
// import scipy.optimize
// from scipy.spatial.distance import cdist, euclidean
//
//
// def geometric_median(x, eps=1e-5):
//     X = eval(x)
//     y = np.mean(X, 0)
//
//     while True:
//         D = cdist(X, [y])
//         nonzeros = (D != 0)[:, 0]
//
//         Dinv = 1 / D[nonzeros]
//         Dinvs = np.sum(Dinv)
//         W = Dinv / Dinvs
//         T = np.sum(W * X[nonzeros], 0)
//
//         num_zeros = len(X) - np.sum(nonzeros)
//         if num_zeros == 0:
//             y1 = T
//         elif num_zeros == len(X):
//             return f'{round(y[0])},{round(y[1])},{round(y[2])},{round(y[3])}'
//         else:
//             R = (T - y) * Dinvs
//             r = np.linalg.norm(R)
//             rinv = 0 if r == 0 else num_zeros/r
//             y1 = max(0, 1-rinv)*T + min(1, rinv)*y
//
//         if euclidean(y, y1) < eps:
//             return f'{round(y1[0])},{round(y1[1])},{round(y1[2])},{round(y1[3])}'
//
//         y = y1");
//     }
//
//     private async Task Init()
//     {
//         Installer.InstallPath = Path.GetFullPath(".");
//         Installer.LogMessage += Console.WriteLine;
//         await Installer.SetupPython();
//
//         Installer.TryInstallPip();
//         Installer.PipInstallModule("numpy");
//         Installer.PipInstallModule("scipy");
//
//         PythonEngine.Initialize();
//     }
//
//     public Rgba GetGeometricMedian(IEnumerable<Rgba> pixels)
//     {
//         string result = module.geometric_median($"np.array([{string.Join(", ", pixels.Select(x => x.ToString()))}])").ToString();
//
//         var res = result.Split(",").Select(int.Parse).ToArray();
//
//         return new Rgba(res[0], res[1], res[2], res[3]);
//     }
// }
