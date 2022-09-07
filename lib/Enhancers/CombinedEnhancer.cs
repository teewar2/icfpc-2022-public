using System;
using System.Collections.Generic;

namespace lib.Enhancers;

public class CombinedEnhancer : ISolutionEnhancer
{
    private readonly ISolutionEnhancer[] enhancers;

    public CombinedEnhancer(params ISolutionEnhancer[] enhancers)
    {
        this.enhancers = enhancers;
    }

    public List<Move> Enhance(Screen problem, List<Move> moves)
    {
        var bestScore = GetScore(problem, moves);

        while (true)
        {
            foreach (var enhancer in enhancers)
                moves = enhancer.Enhance(problem, moves);
            var score = GetScore(problem, moves);
            if (score < bestScore)
            {
                bestScore = score;
                continue;
            }

            if (score > bestScore)
                throw new Exception("Enhancers shouldn't make bad solutions");

            return moves;
        }
    }

    private int GetScore(Screen problem, List<Move> moves)
    {
        var canvas = new Canvas(problem);
        ApplyRange(canvas, moves, 0, moves.Count - 1);
        return canvas.GetScore(problem);
    }

    private void ApplyRange(Canvas canvas, List<Move> moves, int start, int end)
    {
        for (var i = start; i <= end; i++)
        {
            var move = moves[i];
            canvas.Apply(move);
        }
    }
}
