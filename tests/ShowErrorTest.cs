using System;
using System.Linq;
using lib;
using NUnit.Framework;

namespace tests;

public class ShowErrorTest
{
    [Test]
    public void ShowError([Range(37, 37)]int problemId)
    {
        var problem = Screen.LoadProblem(problemId);
        var canvas = new Canvas(problem);
        var diffSum = 0.0;
        foreach (var b in canvas.Flatten())
        {
            for (int dx = 0; dx < b.Width; dx++)
            for (int dy = 0; dy < b.Height; dy++)
            {
                var x = b.Left + dx;
                var y = b.Bottom + dy;
                var diff = problem.Pixels[x, y].DiffTo(b.ColorAt(x, (y+16+400)%400));
                problem.Pixels[x, y] = new Rgba((int)Math.Min(255, diff / Math.Sqrt(3)), 0, 0, 255);
                diffSum += diff;
            }
        }

        Console.WriteLine(diffSum * 0.005);

        var cols = problem.InitialBlocks.Select(b => b.Left).Distinct().Count();
        var grid = GridBuilder.BuildRegularGrid(problem, cols, cols);
        problem.ToImage($"error-{problemId}.png", grid);
    }

}
