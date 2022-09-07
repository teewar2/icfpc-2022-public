using System.Collections.Generic;
using System.Linq;

namespace lib.Enhancers;

public class CutEnhancer : ISolutionEnhancer
{
    public List<Move> Enhance(Screen problem, List<Move> moves)
    {
        moves = EnhanceDelta(problem, moves, 3);
        moves = EnhanceDelta(problem, moves, 1);
        return moves;
    }

    private List<Move> EnhanceDelta(Screen problem, List<Move> moves, int delta)
    {
        var cutIndexes = moves.Select((move, i) => new { move, i }).Where(x => x.move is CutMove).Select(x => x.i).ToArray();
        if (!cutIndexes.Any())
            return moves;

        var bestScore = GetScore(problem, moves);

        while (true)
        {
            Move? bestCut = null;

            var canvas = new Canvas(problem);
            foreach (var cutIndex in cutIndexes)
            {
                var copy = canvas.Copy();
                ApplyRange(copy, moves, 0, cutIndex - 1);
                foreach (var cut in IterateCuts(copy, moves[cutIndex], delta))
                {
                    var copy2 = copy.Copy();
                    copy2.Apply(cut);
                    try
                    {
                        ApplyRange(copy2, moves, cutIndex + 1, moves.Count - 1);
                    }
                    catch (BadBlockException)
                    {
                        continue;
                    }
                    catch (BadMoveException)
                    {
                        continue;
                    }

                    var score = copy2.GetScore(problem);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestCut = cut;
                        moves = moves.ToList();
                        moves[cutIndex] = bestCut;
                        break;
                    }
                }
            }

            if (bestCut == null)
                return moves;
        }
    }

    private IEnumerable<Move> IterateCuts(Canvas canvas, Move move, int delta)
    {
        var cut = (CutMove)move;
        var block = canvas.Blocks[cut.BlockId];
        if (cut is VCutMove vCut)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                var x = vCut.LineNumber + dx * delta;
                if (x > block.Left && x < block.Right)
                    yield return vCut with { LineNumber = x };
            }
        }
        else if (cut is HCutMove hCut)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                var y = hCut.LineNumber + dy * delta;
                if (y > block.Bottom && y < block.Top)
                    yield return hCut with { LineNumber = y };
            }
        }
        else if (cut is PCutMove pCut)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                var v = pCut.Point + new V(dx * delta, dy * delta);
                if (v.IsStrictlyInside(block))
                    yield return pCut with { Point = v };
            }
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
