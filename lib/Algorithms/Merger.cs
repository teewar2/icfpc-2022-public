using System;
using System.Collections.Generic;

namespace lib.Algorithms;

public static class Merger
{
    public static List<Move> MergeAllAndApplyExistedSolution(this Screen screen, List<Move> existedSolution)
    {
        var result = screen.MergeAll();
        var moves = result.Moves;
        var colorMove = new ColorMove(result.Canvas.TopLevelIdCounter.ToString(), new Rgba(255, 255, 255, 255));
        moves.Add(colorMove);
        result.Canvas.ApplyColor(colorMove);

        var idShift = result.Canvas.TopLevelIdCounter;
        string ShiftId(string blockId)
        {
            var parts = blockId.Split(new[] { '.' }, 2);
            var shiftedId = parts[0].ToInt() + idShift;
            if (parts.Length == 1) return shiftedId.ToString();
            return $"{shiftedId}.{parts[1]}";
        }

        foreach (var move in existedSolution)
        {
            var newMove = move switch
            {
                NopMove => move,
                ColorMove m => m with { BlockId = ShiftId(m.BlockId) },
                HCutMove m => m with { BlockId = ShiftId(m.BlockId) },
                PCutMove m => m with { BlockId = ShiftId(m.BlockId) },
                VCutMove m => m with { BlockId = ShiftId(m.BlockId) },
                MergeMove m => m with { Block1Id = ShiftId(m.Block1Id), Block2Id = ShiftId(m.Block2Id) },
                SwapMove m => m with { Block1Id = ShiftId(m.Block1Id), Block2Id = ShiftId(m.Block2Id) },
                _ => throw new ArgumentOutOfRangeException(nameof(move))
            };
            moves.Add(newMove);
        }
        return moves;
    }

    public static MergeResult MergeAll(this Screen screen)
    {
        var length = screen.InitialBlocks.Length;
        var size = (int)Math.Round(Math.Sqrt(length));
        var canvas = new Canvas(screen);
        var id = length - 1;
        var commands = new List<Move>();
        var starting = 0;
        var columns = new List<int>();
        var mergeCost = 0;
        void Merge(int b1, int b2)
        {
            var mergeMove = new MergeMove(b1.ToString(), b2.ToString());
            mergeCost += mergeMove.GetCost(canvas);
            canvas.ApplyMerge(mergeMove);
            commands.Add(mergeMove);
            id++;
        }
        for (var j = 0; j < size; j++) {
            Merge(starting, starting + 1);
            for (var i = 1; i < size - 1; i++) {
                Merge(id, size * j + i + 1);
            }
            columns.Add(id);
            starting = size * (j + 1);
        }
        Merge(columns[0], columns[1]);
        for (var i = 1; i < size - 1; i++) {
            Merge(id, columns[i + 1]);
        }
        return new MergeResult(commands, mergeCost, canvas);
    }
}

public class MergeResult
{
    public MergeResult(List<Move> moves, int eraseCost, Canvas canvas)
    {
        Canvas = canvas;
        Moves = moves;
        EraseCost = eraseCost;
    }

    public Canvas Canvas;
    public List<Move> Moves;
    public int EraseCost;
}
