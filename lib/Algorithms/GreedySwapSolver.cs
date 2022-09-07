using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public static class GreedySwapSolver
{
    public static List<Move> Solve(Screen problem)
    {
        var canvas = new Canvas(problem);

        var moves = new List<Move>();

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

        return moves;
    }
}
