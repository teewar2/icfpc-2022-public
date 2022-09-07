using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using lib;
using lib.Algorithms;
using lib.db;
using lib.Enhancers;
using NUnit.Framework;
using TextCopy;

namespace tests;

public class GridGuidedPainterTests
{
    [Test]
    [Parallelizable(ParallelScope.All)]
    public async Task Run([Range(10, 10)] int problemId)
    {
        var bestScore = double.PositiveInfinity;

        foreach (var colorTolerance in new[]{50})
        foreach (var size in new[]{17})
        {
            var screen = Screen.LoadProblem(problemId);
            var grid = CreateRegularGrid(size);

            var res = GridGuidedPainterRunner.Solve(problemId, size, size);
            var (moves, score, canvas) = (res.Moves, res.Score, res.Canvas);

            // var solver = new GridGuidedPainter(grid, screen, colorTolerance);
            // var (moves, score, canvas) = solver.GetBestResultWithCanvas();

            var solution = moves.StrJoin("\n");
            //List<Move> moves2 = Enhancer.Enhance2(screen, moves);
            //score = screen.GetScore(moves2);
            if (score < bestScore)
            {
                Console.WriteLine($"Score: {score} = {canvas.GetSimilarity(screen)} + {canvas.TotalCost} cellSize: {size} colorTolerance: {colorTolerance} movesCount: {moves.Count}");
                bestScore = score;
                await ClipboardService.SetTextAsync(solution);
                await SolutionRepo.SubmitAsync(new ContestSolution(problemId, (int)score, solution, new SolverMeta(),"RegularGrid"));
            }
        }
    }

    private Grid CreateRegularGrid(int count)
    {
        var rows = new List<GridRow>();
        for (int y = 0; y < count; y++)
        {
            var cells = new List<GridCell>();
            for (int x = 0; x < count; x++)
            {
                cells.Add(new GridCell(400/count));
            }

            var height = 400/count;
            rows.Add(new GridRow(height, cells));
        }
        return new Grid(rows);

    }
}
