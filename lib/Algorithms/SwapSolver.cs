using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public static class SwapSolver
{
    public static List<Move> Solve(Screen problem)
    {
        var canvas = new Canvas(problem);

        var moves = new List<Move>();
        MergeV(problem, canvas, moves, 20);
        // MergeH(problem, canvas, moves, 4);
        Swap(problem, canvas, moves);

        return moves;
    }

    public static void GreedySwap(Screen problem, Canvas canvas, List<Move> moves)
    {
        while (true)
        {
            var bestEstimation = 0.0;
            SwapMove? bestMove = null;
            var blocks = canvas.Blocks.Values.OrderBy(x => x.Left).ThenBy(x => x.Bottom).ToArray();
            for (int i = 0; i < blocks.Length - 1; i++)
            {
                for (int j = i + 1; j < blocks.Length; j++)
                {
                    var swapMove = new SwapMove(blocks[i].Id, blocks[j].Id);

                    double scoreDiff = swapMove.GetCost(canvas);
                    scoreDiff -= problem.DiffTo(blocks[i]);
                    scoreDiff += problem.DiffTo(blocks[j].MoveTo(blocks[i]));

                    scoreDiff -= problem.DiffTo(blocks[j]);
                    scoreDiff += problem.DiffTo(blocks[i].MoveTo(blocks[j]));

                    if (scoreDiff < bestEstimation)
                    {
                        bestEstimation = scoreDiff;
                        bestMove = swapMove;
                    }
                }
            }

            if (bestMove == null)
                break;

            moves.Add(bestMove);
            canvas.Apply(bestMove);
        }
    }

    public static void MergeV(Screen problem, Canvas canvas, List<Move> moves, int count = 2)
    {
        Merge(problem, canvas, moves, true, count);
    }

    public static void MergeH(Screen problem, Canvas canvas, List<Move> moves, int count = 2)
    {
        Merge(problem, canvas, moves, false, count);
    }

    public static void Merge(Screen problem, Canvas canvas, List<Move> moves, bool vert, int count = 2)
    {
        var blocks = vert
            ? canvas.Blocks.Values.OrderBy(x => x.Left).ThenBy(x => x.Bottom).ToArray()
            : canvas.Blocks.Values.OrderBy(x => x.Bottom).ThenBy(x => x.Left).ToArray();
        for (int i = 0; i < blocks.Length; i += count)
        {
            var id = blocks[i].Id;
            for (int j = i + 1; j < i + count; j++)
            {
                var mergeMove = new MergeMove(id, blocks[j].Id);
                moves.Add(mergeMove);
                var complexBlock = canvas.ApplyMerge(mergeMove);
                id = complexBlock.Id;
            }
        }
    }

    public static void Swap(Screen problem, Canvas canvas, List<Move> moves)
    {
        var blocks = canvas.Blocks.Values.OrderBy(x => x.Left).ThenBy(x => x.Bottom).ToArray();
        var blockIdToPosition = blocks.Select((x, i) => (x, i)).ToDictionary(x => x.x.Id, x => x.i);


        var w = new double[blocks.Length, blocks.Length];
        for (int i = 0; i < blocks.Length; i++)
        for (int j = 0; j < blocks.Length; j++)
        {
            var block = blocks[i].MoveTo(blocks[j]);

            //w[i, j] = problem.DiffTo(block);
            w[i, j] = problem.DiffTo(block)
                      + (i == j
                          ? 0
                          : Move.GetCost(canvas.ScalarSize, block.ScalarSize, 3) * 0.01);
        }

        var indexes = Enumerable.Range(0, blocks.Length).ToArray();
        var positions = MinMatchFinder.FindMinMatch(w);
        Array.Sort(positions, indexes);

        var simpleBlocksToMove = indexes.Select(x => blocks[x]).ToArray();

        for (int i = 0; i < simpleBlocksToMove.Length; i++)
        {
            var p1 = blockIdToPosition[simpleBlocksToMove[i].Id];
            var p2 = i;
            if (p1 == p2)
            {
                Console.WriteLine($"SKIP {simpleBlocksToMove[i].Id}");
                continue;
            }

            moves.Add(new SwapMove(blocks[i].Id, simpleBlocksToMove[i].Id));

            blockIdToPosition[blocks[i].Id] = p1;
            blockIdToPosition[simpleBlocksToMove[i].Id] = p2;

            (blocks[p1], blocks[p2]) = (blocks[p2], blocks[p1]);
        }
    }
}
