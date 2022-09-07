using System;
using System.Collections.Generic;
using System.Linq;

namespace lib;

public class BadBlockException : Exception
{
    public BadBlockException(string message)
        : base(message)
    {
    }
}

public abstract record Block
{
    public string Id { get; set; }
    public V BottomLeft { get; set; }
    public V TopRight { get; set; }

    protected Block(string id, V bottomLeft, V topRight)
    {
        Id = id;
        BottomLeft = bottomLeft;
        TopRight = topRight;
        if (bottomLeft.X >= topRight.X || bottomLeft.Y >= topRight.Y)
            throw new BadBlockException($"Bad block {bottomLeft} {topRight}");
    }

    public V Size => TopRight - BottomLeft;
    public int Height => TopRight.Y - BottomLeft.Y;
    public int Width => TopRight.X - BottomLeft.X;
    public int Left => BottomLeft.X;
    public int Right => TopRight.X;
    public int Bottom => BottomLeft.Y;
    public int Top => TopRight.Y;
    public int ScalarSize => Size.GetScalarSize();
    public bool IntersectsWith(V bottomLeft, int width, int height)
    {
        var left = Math.Max(bottomLeft.X, Left);
        var right = Math.Min(bottomLeft.X+width, Right);
        var bottom = Math.Max(bottomLeft.Y, Bottom);
        var top = Math.Min(bottomLeft.Y+height, Top);
        return left < right && bottom < top;
    }
    public abstract IEnumerable<AtomicBlock> GetChildren();
    public Block MoveTo(Block other) => MoveTo(other.BottomLeft, other.TopRight);
    public abstract Block MoveTo(V bottomLeft, V topRight);

    public abstract bool IsFilledWithColor(Rgba color, V bottomLeft, int width, int height, double colorTolerance);
}

public abstract record AtomicBlock(string Id, V BottomLeft, V TopRight) : Block(Id, BottomLeft, TopRight)
{
    public abstract AtomicBlock Cut(string newBlockId, V newBottomLeft, V newTopRight);

    public abstract Rgba ColorAt(int x, int y);
}

public record PngBlock(string Id, V BottomLeft, V TopRight, V PngBottomLeft, Rgba[,] SourcePng) : AtomicBlock(Id, BottomLeft, TopRight)
{
    public override IEnumerable<AtomicBlock> GetChildren() => new[] { this };

    public override Block MoveTo(V bottomLeft, V topRight)
    {
        if (topRight - bottomLeft != Size)
            throw new Exception("topRight - bottomLeft != Size");

        return this with {BottomLeft = bottomLeft, TopRight = topRight};
    }

    public override bool IsFilledWithColor(Rgba color, V bottomLeft, int width, int height, double colorTolerance)
    {
        for (int dx = 0; dx < width; dx++)
        for (int dy = 0; dy < height; dy++)
        {
            var localP = bottomLeft + new V(dx, dy) - BottomLeft;
            if (localP.X >= 0
                && localP.X < Width
                && localP.Y >= 0
                && localP.Y < Height)
            {
                var pngP = localP + PngBottomLeft;
                var sourcePngPixel = SourcePng[pngP.X, pngP.Y];
                if (sourcePngPixel.DiffTo(color) > colorTolerance) return false;
            }
        }
        return true;
    }

    public override PngBlock Cut(string newBlockId, V newBottomLeft, V newTopRight)
    {
        return this with
        {
            Id = newBlockId,
            BottomLeft = newBottomLeft,
            TopRight = newTopRight,
            PngBottomLeft = PngBottomLeft + newBottomLeft - BottomLeft
        };
    }

    public override Rgba ColorAt(int x, int y)
    {
        var v = PngBottomLeft + new V(x, y) - BottomLeft;
        return SourcePng[v.X, v.Y];
    }
}

public record SimpleBlock(string Id, V BottomLeft, V TopRight, Rgba Color) : AtomicBlock(Id, BottomLeft, TopRight)
{
    public override IEnumerable<AtomicBlock> GetChildren() => new[] { this };

    public override Block MoveTo(V bottomLeft, V topRight)
    {
        if (topRight - bottomLeft != Size)
            throw new Exception("topRight - bottomLeft != Size");

        return this with {BottomLeft = bottomLeft, TopRight = topRight};
    }
    public override SimpleBlock Cut(string newBlockId, V newBottomLeft, V newTopRight)
    {
        return this with
        {
            Id = newBlockId,
            BottomLeft = newBottomLeft,
            TopRight = newTopRight,
        };
    }

    public override Rgba ColorAt(int x, int y) => Color;

    public override bool IsFilledWithColor(Rgba color, V bottomLeft, int width, int height, double colorTolerance) => color.DiffTo(Color) <= colorTolerance;
}

public record ComplexBlock(string Id, V BottomLeft, V TopRight, AtomicBlock[] Children) : Block(Id, BottomLeft, TopRight)
{
    public override IEnumerable<AtomicBlock> GetChildren() => Children;

    public override Block MoveTo(V bottomLeft, V topRight)
    {
        if (topRight - bottomLeft != Size)
            throw new Exception("topRight - bottomLeft != Size");

        var diff = bottomLeft - BottomLeft;
        return this with
        {
            BottomLeft = bottomLeft,
            TopRight = topRight,
            Children = Children.Select(x => (AtomicBlock)x.MoveTo(x.BottomLeft + diff, x.TopRight + diff)).ToArray()
        };
    }
    public override bool IsFilledWithColor(Rgba color, V bottomLeft, int width, int height, double colorTolerance)
    {
        return !Children.Any(child => child.IntersectsWith(bottomLeft, width, height) && !child.IsFilledWithColor(color, bottomLeft, width, height, colorTolerance));
    }
}
