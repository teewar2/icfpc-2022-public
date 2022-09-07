using System;
using System.IO;
using lib;
using lib.Algorithms;
using NUnit.Framework;

namespace tests;

[TestFixture]
public class GridBuilderTests
{
    [Test]
    public void TestBuild()
    {
        var problem = Screen.LoadProblem(1);
        var grid = GridBuilder.BuildOptimalGrid(problem);

    }

    [Test]
    public void TestOptimizeRowHeights()
    {
        GridBuilder.estimations = 0;

        var problem = Screen.LoadProblem(39);
        problem = Rotator.Rotate(problem, 0);

        var grid = GridBuilder.BuildRegularGrid(problem, 40, 40);
        double estimation;

        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "regular.png"), grid);

        (grid, estimation) = GridBuilder.OptimizeRowHeights(problem, grid);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedR.png"), grid);
        Console.Out.WriteLine($"rows={estimation}");

        (grid, estimation) = GridBuilder.OptimizeCellWidths(problem, grid);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedC.png"), grid);
        Console.Out.WriteLine($"cells={estimation}");

        var (moves, score) = new GridGuidedPainter(grid, problem).GetBestResult();
        problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "solved0.png"));
        Console.Out.WriteLine(score);

        (grid, estimation) = GridBuilder.OptimizeCellsViaMerge(problem, grid);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedM.png"), grid);
        Console.Out.WriteLine($"merge={estimation}");
        // for (int i = 0; i < grid.Rows.Count; i++)
        // {
        //     (grid, estimation) = GridBuilder.OptimizeCellsViaMerge(problem, grid, i);
        //     problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), $"optimizedM{i}.png"), grid);
        //     Console.Out.WriteLine($"merge{i}={estimation}");
        // }

        (moves, score) = new GridGuidedPainter(grid, problem).GetBestResult();
        problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "solved_after_merge.png"));
        Console.Out.WriteLine(score);

        (grid, estimation) = GridBuilder.OptimizeRowsViaMerge(problem, grid);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedRVM.png"), grid);
        Console.Out.WriteLine($"rowsVM={estimation}");

        (moves, score) = new GridGuidedPainter(grid, problem).GetBestResult();
        problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "solved_RWM.png"));
        Console.Out.WriteLine(score);

        (grid, estimation) = GridBuilder.OptimizeRowHeights(problem, grid);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedR2.png"), grid);
        Console.Out.WriteLine($"rows={estimation}");

        (grid, estimation) = GridBuilder.OptimizeCellWidths(problem, grid);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedC2.png"), grid);
        Console.Out.WriteLine($"cells={estimation}");

        (moves, score) = new GridGuidedPainter(grid, problem).GetBestResult();
        problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "solved_after_merge_and_move2.png"));
        Console.Out.WriteLine(score);

        (grid, estimation) = GridBuilder.OptimizeRowHeights(problem, grid);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedR3.png"), grid);
        Console.Out.WriteLine($"rows={estimation}");

        (grid, estimation) = GridBuilder.OptimizeCellWidths(problem, grid);
        problem.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "optimizedC3.png"), grid);
        Console.Out.WriteLine($"cells={estimation}");

        (moves, score) = new GridGuidedPainter(grid, problem).GetBestResult();
        problem.MovesToImage(moves, Path.Combine(FileHelper.FindDirectoryUpwards("tests"), "solved_after_merge_and_move3.png"));
        Console.Out.WriteLine(score);


        Console.WriteLine($"estimations={GridBuilder.estimations}");
        Console.WriteLine(moves.StrJoin("\n"));
    }
}
