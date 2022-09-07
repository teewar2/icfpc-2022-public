using System.Collections.Generic;
using System.Linq;

namespace lib.Enhancers;

public class ColorEnhancer : ISolutionEnhancer
{
    public List<Move> Enhance(Screen problem, List<Move> moves)
    {
        moves = EnhanceDelta(problem, moves, 50);
        moves = EnhanceDelta(problem, moves, 20);
        moves = EnhanceDelta(problem, moves, 10);
        moves = EnhanceDelta(problem, moves, 5);
        moves = EnhanceDelta(problem, moves, 1);
        return moves;
    }

    private List<Move> EnhanceDelta(Screen problem, List<Move> moves, int delta)
    {
        var colorIndexes = moves.Select((move, i) => new { move, i }).Where(x => x.move is ColorMove).Select(x => x.i).ToArray();
        if (!colorIndexes.Any())
            return moves;

        var bestScore = GetScore(problem, moves);

        while (true)
        {
            Move? bestCol = null;

            var canvas = new Canvas(problem);
            foreach (var colorIndex in colorIndexes)
            {
                var copy = canvas.Copy();
                ApplyRange(copy, moves, 0, colorIndex - 1);
                foreach (var col in IterateColors(moves[colorIndex], delta))
                {
                    var copy2 = copy.Copy();
                    copy2.Apply(col);
                    try
                    {
                        ApplyRange(copy2, moves, colorIndex + 1, moves.Count - 1);
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
                        bestCol = col;
                        moves = moves.ToList();
                        moves[colorIndex] = bestCol;
                        break;
                    }
                }
            }

            if (bestCol == null)
                break;
        }

        return moves;
    }

    private IEnumerable<Move> IterateColors(Move move, int delta)
    {
        var col = (ColorMove)move;
        for (int dr = -1; dr <= 1; dr++)
        for (int dg = -1; dg <= 1; dg++)
        for (int db = -1; db <= 1; db++)
        {
            var r = col.Color.R + dr * delta;
            var g = col.Color.G + dg * delta;
            var b = col.Color.B + db * delta;
            var a = col.Color.A;
            if (r is >= 0 and <= 255
                && g is >= 0 and <= 255
                && b is >= 0 and <= 255)
                yield return col with { Color = new Rgba(r, g, b, a) };
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
