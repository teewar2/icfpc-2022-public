using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using lib;
using lib.Algorithms;
using NUnit.Framework;

namespace tests;

public class SimpleAlgoTests
{
    [Test]
    public void Run([Range(16, 20)] int problemId)
    {
        var problem = Screen.LoadProblem(problemId);
        var algorithm = new SimpleAlgorithm();

        var (moves, _) = algorithm.Solve(problem);

        var canvas = new Canvas(problem);
        foreach (var move in moves)
        {
            Console.WriteLine(move);
            canvas.Apply(move);
        }

        var score = canvas.GetScore(problem);
        Console.WriteLine(score);
        canvas.ToScreen().ToImage("res.png");
    }
}
