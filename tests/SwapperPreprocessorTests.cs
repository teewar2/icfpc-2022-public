using System;
using System.IO;
using System.Linq;
using lib;
using lib.Algorithms;
using NUnit.Framework;
using TextCopy;

namespace tests;

public class SwapperPreprocessorTests
{
    [Test]
    public void Run()
    {
        var problem = Screen.LoadProblem(2);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "swapper_0.png"));


        var original = GridGuidedPainterRunner.Solve(5, problem, 20, 20);
        Console.Out.WriteLine("originalScore=" + original.Score);

        var (preprocessed, rows) = SwapperPreprocessor.Preprocess(problem, 4);

        preprocessed.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "swapper_1.png"));

        Console.Out.WriteLine(rows.StrJoin(", "));

        var result = GridGuidedPainterRunner.Solve(5, preprocessed, 20, 20);
        var moves = result.Moves.ToList();
        moves.AddRange(SwapperPreprocessor.Postprocess(rows, result.Canvas));

        Console.Out.WriteLine("score=" + problem.GetScore(moves));

        ClipboardService.SetText(moves.StrJoin("\n"));



        // var score = problem.CalculateScore(moves);
        // Console.WriteLine($"Score: {score}, moves: {moves.Count}");
        // var canvas = new Canvas(problem);
        // for (var i = 0; i < moves.Count; i++)
        // {
        //     var move = moves[i];
        //     canvas.Apply(move);
        //     canvas.ToScreen().ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), $"swap{i:000}.png"));
        // }
        //
        // Console.WriteLine(moves.StrJoin("\n"));
        // problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "swap_result.png"));
    }

}
