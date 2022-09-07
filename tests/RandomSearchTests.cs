using System;
using System.Linq;
using lib;
using lib.Algorithms;
using lib.db;
using NUnit.Framework;

namespace tests;

public class RandomSearchTests
{
    [Test]
    public void RunAll()
    {
        for (int i = 1; i <= 15; i++)
        {
            Run(i);
        }
    }

    [TestCase(1)]
    public void Run(int problemNumber)
    {
        var problem = Screen.LoadProblem(problemNumber);
        var moves = new RandomSearchAlgorithm().Solve(problem).ToList();
        var canvas = new Canvas(problem);
        Console.WriteLine("------------");
        foreach (var move in moves)
        {
            Console.WriteLine(move);
            canvas.Apply(move);
        }

        // var commands = string.Join('\n', moves.Select(m => m.ToString()));
        // var solution = new ContestSolution(problemNumber, canvas.GetScore(problem),
        //     commands, new SolverMeta(), DateTime.UtcNow, nameof(RandomSearchAlgorithm));
        // SolutionRepo.Submit(solution).GetAwaiter().GetResult();

        Console.WriteLine("------------");
        Console.WriteLine(canvas.GetScore(problem));
        canvas.ToScreen().ToImage($"res{problemNumber}.png");

    }

    [Test]
    public void Run222()
    {
        var problem = Screen.LoadProblem(1);
        var moves = new RandomSearchAlgorithm(1569227620).Solve(problem).Where(x => x is not NopMove).Take(10).ToList();
        var canvas = new Canvas(problem);
        Console.WriteLine(canvas.GetScore(problem));
        foreach (var move in moves)
        {
            Console.WriteLine(move);
            canvas.Apply(move);
        }

        Console.WriteLine();
        Console.WriteLine(canvas.GetScore(problem));
        canvas.ToScreen().ToImage($"res1a.png");
    }
}
