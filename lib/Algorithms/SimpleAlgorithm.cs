using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public class SimpleAlgorithm : IAlgorithm
{
    public const int CanvasSize = 400;

    public (IList<Move> Moves, double Score) Solve(Screen screen)
    {
        var (bestResult, bestScore) = (new List<Move>(), double.MaxValue);

        for (var size = 25; size <= CanvasSize; size *= 2)
        {
            var (result, score) = GetResult(screen, size);
            if (score < bestScore)
            {
                bestResult = result;
                bestScore = score;
            }
        }

        return (bestResult, bestScore);
    }

    private (List<Move> Moves, double Score) GetResult(Screen screen, int maxBlockSize)
    {
        var resultMoves = new List<Move>();

        var canvas = new Canvas(screen);

        while (true)
        {
            var block = GetBlock(canvas, maxBlockSize * 2);
            if (block is null) break;

            var cutMove = new PCutMove(block.Id, block.BottomLeft + block.Size / 2);
            canvas.ApplyPCut(cutMove);
            resultMoves.Add(cutMove);
        }

        var blocks = canvas.Blocks.Values.ToList();
        foreach (var block in blocks)
        {
            var averageBlockColor = screen.GetAverageColorByGeometricMedian(block);
            var colorMove = new ColorMove(block.Id, averageBlockColor);

            var currentScore = canvas.GetScore(screen);
            var currentBlocks = canvas.Blocks;
            canvas.ApplyColor(colorMove);

            var newScore = canvas.GetScore(screen);
            if (newScore < currentScore)
            {
                resultMoves.Add(colorMove);
            }
            else
            {
                canvas.Blocks = currentBlocks;
            }
        }

        var totalScore = canvas.GetScore(screen);
        return (resultMoves, totalScore);
    }

    private Block? GetBlock(Canvas canvas, int minSize)
    {
        return canvas.Blocks.Values
            .Where(x => x.Size.X >= minSize && x.Size.Y >= minSize)
            .FirstOrDefault();
    }
}
