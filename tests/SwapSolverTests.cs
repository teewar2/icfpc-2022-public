using System;
using System.IO;
using System.Linq;
using lib;
using lib.Algorithms;
using NUnit.Framework;

namespace tests;

public class SwapSolverTests
{
    [Test]
    public void Run()
    {
        var problem = Screen.LoadProblem(30);
        problem.MovesToImage(Enumerable.Empty<Move>(), Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "swap.png"));

        var moves = SwapSolver.Solve(problem);

        var score = problem.CalculateScore(moves);
        Console.WriteLine($"Score: {score}, moves: {moves.Count}");
        var canvas = new Canvas(problem);
        for (var i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            canvas.Apply(move);
            canvas.ToScreen().ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), $"swap{i:000}.png"));
        }

        Console.WriteLine(moves.StrJoin("\n"));
        problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "swap_result.png"));
    }

    [Test]
    public void Greedy()
    {
        var problem = Screen.LoadProblem(31);
        problem.MovesToImage(Enumerable.Empty<Move>(), Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "swap.png"));

        var moves = GreedySwapSolver.Solve(problem);

        var score = problem.CalculateScore(moves);
        Console.WriteLine($"Score: {score}, moves: {moves.Count}");
        var canvas = new Canvas(problem);
        for (var i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            canvas.Apply(move);
            canvas.ToScreen().ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), $"swap{i:000}.png"));
        }

        Console.WriteLine(moves.StrJoin("\n"));
        problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "swap_result.png"));
    }
}
