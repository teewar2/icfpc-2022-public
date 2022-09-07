using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace lib;

public static class Moves
{
    public static List<Move> Parse(string program)
    {
        var result = new List<Move>();
        foreach (var line in program.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("#")))
            result.Add(Move.Parse(line));
        return result;
    }
}

public abstract record Move
{
    public static int GetCost(int canvasSize, int blockSize, int baseCost)
    {
        return (int)Math.Round((double)baseCost * canvasSize / blockSize);
    }

    public int GetCost(Canvas canvas)
    {
        return GetCost(canvas.ScalarSize, GetBlockScalarSize(canvas), GetBaseCost(canvas));
    }

    protected abstract int GetBlockScalarSize(Canvas canvas);

    protected abstract int GetBaseCost(Canvas canvas);

    public static Move Parse(string s)
    {
        return (Move?)ColorMove.TryParse(s)
               ?? (Move?)HCutMove.TryParse(s)
               ?? (Move?)VCutMove.TryParse(s)
               ?? (Move?)PCutMove.TryParse(s)
               ?? (Move?)SwapMove.TryParse(s)
               ?? (Move?)MergeMove.TryParse(s)
               ?? throw new Exception($"Bad move: {s}");
    }
}

public record NopMove(string Comment = "") : Move()
{
    protected override int GetBlockScalarSize(Canvas canvas) => 1;

    protected override int GetBaseCost(Canvas canvas) => 0;

    public override string ToString()
    {
        return "# " + Comment;
    }
}

public record ColorMove(string BlockId, Rgba Color) : Move()
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return canvas.Blocks[BlockId].ScalarSize;
    }

    protected override int GetBaseCost(Canvas canvas) => 5;

    public override string ToString() => $"color [{BlockId}] {Color}";

    public static ColorMove? TryParse(string s)
    {
        var re = new Regex(@"^color\s*\[(?<blockId>[^]]*)\]\s*\[(?<r>\d+),\s*(?<g>\d+),\s*(?<b>\d+),\s*(?<a>\d+)\]$");
        var m = re.Match(s.ToLower());
        if (!m.Success)
            return null;
        return new ColorMove(
            m.Groups["blockId"].Value,
            new Rgba(
                int.Parse(m.Groups["r"].Value),
                int.Parse(m.Groups["g"].Value),
                int.Parse(m.Groups["b"].Value),
                int.Parse(m.Groups["a"].Value)
            )
        );
    }
}

public abstract record CutMove(string BlockId) : Move;

public record PCutMove(string BlockId, V Point) : CutMove(BlockId)
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return canvas.Blocks[BlockId].ScalarSize;
    }

    protected override int GetBaseCost(Canvas canvas)
    {
        return canvas.Problem.GetPointCutCost();
    }

    public override string ToString() => $"cut [{BlockId}] {Point}";

    public static PCutMove? TryParse(string s)
    {
        var re = new Regex(@"^cut\s*\[(?<blockId>[^]]*)\]\s*\[(?<x>\d+),\s*(?<y>\d+)\]$");
        var m = re.Match(s.ToLower());
        if (!m.Success)
            return null;
        return new PCutMove(
            m.Groups["blockId"].Value,
            new V(
                int.Parse(m.Groups["x"].Value),
                int.Parse(m.Groups["y"].Value)
            )
        );
    }
}

public record HCutMove(string BlockId, int LineNumber) : CutMove(BlockId)
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return canvas.Blocks[BlockId].ScalarSize;
    }

    protected override int GetBaseCost(Canvas canvas)
    {
        return canvas.Problem.GetLineCutCost();
    }

    public override string ToString() => $"cut [{BlockId}] [y] [{LineNumber}]";

    public static HCutMove? TryParse(string s)
    {
        var re = new Regex(@"^cut\s*\[(?<blockId>[^]]*)\]\s*\[y\]\s*\[(?<lineNumber>\d+)\]$");
        var m = re.Match(s.ToLower());
        if (!m.Success)
            return null;
        return new HCutMove(
            m.Groups["blockId"].Value,
            int.Parse(m.Groups["lineNumber"].Value)
        );
    }
}

public record VCutMove(string BlockId, int LineNumber) : CutMove(BlockId)
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return canvas.Blocks[BlockId].ScalarSize;
    }

    protected override int GetBaseCost(Canvas canvas)
    {
        return canvas.Problem.GetLineCutCost();
    }

    public override string ToString() => $"cut [{BlockId}] [x] [{LineNumber}]";

    public static VCutMove? TryParse(string s)
    {
        var re = new Regex(@"^cut\s*\[(?<blockId>[^]]*)\]\s*\[x\]\s*\[(?<lineNumber>\d+)\]$");
        var m = re.Match(s.ToLower());
        if (!m.Success)
            return null;
        return new VCutMove(
            m.Groups["blockId"].Value,
            int.Parse(m.Groups["lineNumber"].Value)
        );
    }
}

public record SwapMove(string Block1Id, string Block2Id) : Move
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return canvas.Blocks[Block1Id].ScalarSize;
    }

    protected override int GetBaseCost(Canvas canvas) => 3;

    public override string ToString() => $"swap [{Block1Id}] [{Block2Id}]";

    public static SwapMove? TryParse(string s)
    {
        var re = new Regex(@"^swap\s*\[(?<block1Id>[^]]*)\]\s*\[(?<block2Id>[^]]*)\]$");
        var m = re.Match(s.ToLower());
        if (!m.Success)
            return null;
        return new SwapMove(
            m.Groups["block1Id"].Value,
            m.Groups["block2Id"].Value
        );
    }
}

public record MergeMove(string Block1Id, string Block2Id) : Move
{
    protected override int GetBlockScalarSize(Canvas canvas)
    {
        return Math.Max(canvas.Blocks[Block1Id].ScalarSize, canvas.Blocks[Block2Id].ScalarSize);
    }

    protected override int GetBaseCost(Canvas canvas) => 1;


    public override string ToString() => $"merge [{Block1Id}] [{Block2Id}]";

    public static MergeMove? TryParse(string s)
    {
        var re = new Regex(@"^merge\s*\[(?<block1Id>[^]]*)\]\s*\[(?<block2Id>[^]]*)\]$");
        var m = re.Match(s.ToLower());
        if (!m.Success)
            return null;
        return new MergeMove(
            m.Groups["block1Id"].Value,
            m.Groups["block2Id"].Value
        );
    }
}
