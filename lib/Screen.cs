using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using lib.Algorithms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace lib;

public class Screen
{
    private const double Alpha = 0.005;
    public Rgba[,] Pixels;
    public AtomicBlock[] InitialBlocks;

    public int Width => Pixels.GetLength(0);
    public int Height => Pixels.GetLength(1);

    public static Screen LoadProblem(int problem)
    {
        var file = Path.Combine(FileHelper.FindDirectoryUpwards("problems"), $"problem{problem}.png");
        using var image = (Image<Rgba32>)Image.Load(file, new PngDecoder());
        var result = LoadFrom(image);
        var jsonFile = Path.ChangeExtension(file, ".json");
        if (File.Exists(jsonFile))
        {
            var json = JsonConvert.DeserializeObject<JToken>(File.ReadAllText(jsonFile))!;
            var width = (int)json["width"]!;
            var height = (int)json["height"]!;
            var initialBlocks = new List<AtomicBlock>();
            foreach (var jToken in json["blocks"]!)
            {
                var blockId = jToken["blockId"]!.ToString();
                var left = (int)jToken["bottomLeft"]![0]!;
                var bottom = (int)jToken["bottomLeft"]![1]!;
                var right = (int)jToken["topRight"]![0]!;
                var top = (int)jToken["topRight"]![1]!;

                var color = jToken["color"];
                if (color != null)
                {
                    var r = (int)color![0]!;
                    var g = (int)color![1]!;
                    var b = (int)color![2]!;
                    var a = (int)color![3]!;
                    initialBlocks.Add(new SimpleBlock(blockId, new V(left, bottom), new V(right, top), new Rgba(r, g, b, a)));
                }
                else
                {
                    var sourcePng = new Rgba[width, height];
                    var colors = jToken["colors"]!.ToObject<int[][]>()!;
                    var pngBottomLeft = jToken["pngBottomLeftPoint"]!.ToObject<int[]>()!;
                    for (int i = 0; i < colors.Length; i++)
                    {
                        var pixel = colors[i];
                        sourcePng[i % 400, 399-i / 400] = new Rgba(pixel[0], pixel[1], pixel[2], pixel[3]);
                    }
                    initialBlocks.Add(new PngBlock(blockId, new V(left, bottom), new V(right, top), new V(pngBottomLeft[0], pngBottomLeft[1]), sourcePng));
                }
            }
            result.InitialBlocks = initialBlocks.ToArray();
        }

        return result;
    }

    public static Screen LoadFrom(Image<Rgba32> bitmap)
    {
        var ps = new Rgba[bitmap.Width, bitmap.Height];
        for (int x = 0; x < bitmap.Width; x++)
        for (int y = 0; y < bitmap.Height; y++)
        {
            var p = bitmap[x, y];
            if (p.A != 255)
                throw new Exception("Alpha is not 255?!?");
            ps[x, bitmap.Height - y - 1] = new Rgba(p.R, p.G, p.B, p.A);
        }
        return new Screen(ps);
    }

    public int GetScore(List<Move> moves)
    {
        var canvas = GetCanvasAfter(moves);
        return canvas.GetScore(this);
    }

    public Canvas GetCanvasAfter(List<Move> moves)
    {
        var canvas = new Canvas(this);
        foreach (var move in moves)
            canvas.Apply(move);
        return canvas;
    }

    public Screen(int width, int height)
        : this(new Rgba[width, height])
    {
    }

    public Screen(Rgba[,] pixels)
    {
        Pixels = pixels;
        InitialBlocks = new[] { new SimpleBlock("0", V.Zero, new V(Width, Height), new Rgba(255, 255, 255, 255)) };
    }

    public double DiffTo(V bottomLeft, V topRight, Rgba color)
    {
        var diff = 0.0;
        for (int x = bottomLeft.X; x < topRight.X; x++)
            if (x < Width)
                for (int y = bottomLeft.Y; y < topRight.Y; y++)
                {
                    if (y >= Height) continue;

                    var p1 = Pixels[x, y];
                    diff += p1.DiffTo(color);
                }
        return diff * Alpha;
    }

    public double DiffTo(Screen other)
    {
        var diff = 0.0;
        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            var p1 = Pixels[x, y];
            var p2 = other.Pixels[x, y];
            diff += p1.DiffTo(p2);
        }
        return diff * Alpha;
    }

    public double DiffTo(SimpleBlock block)
    {
        return DiffTo(block.BottomLeft, block.TopRight, block.Color);
    }

    public double DiffTo(PngBlock block)
    {
        return DiffTo(block, block.BottomLeft, block.Width, block.Height);
    }

    public double DiffTo(Block block)
    {
        return block switch
        {
            SimpleBlock sb => DiffTo(sb),
            PngBlock pb => DiffTo(pb),
            ComplexBlock cb => DiffTo(cb),
            _ => throw new Exception(block.ToString())
        };
    }

    public double DiffTo(ComplexBlock block)
    {
        return block.Children.Sum(DiffTo);
    }

    public Rgba GetAverageColor(Block block)
    {
        var bl = block.BottomLeft;
        var tr = block.TopRight;
        return GetAverageColor(bl, tr);
    }

    public Rgba GetAverageColor(V bl, V tr)
    {
        var pixelsCount = (tr - bl).GetScalarSize();

        var (r, g, b, a) = (0, 0, 0, 0);

        for (int x = bl.X; x < tr.X; x++)
            if (x < Width)
                for (int y = bl.Y; y < tr.Y; y++)
                {
                    if (y >= Height) continue;
                    var pixel = Pixels[x, y];
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                    a += pixel.A;
                }

        return new Rgba(
            (r / pixelsCount),
            (g / pixelsCount),
            (b / pixelsCount),
            (a / pixelsCount));
    }

    public Rgba GetAverageColor2(Block block)
    {
        var pixelsCount = block.ScalarSize;

        var (r, g, b, a) = (0.0, 0.0, 0.0, 0.0);

        for (int x = block.BottomLeft.X; x < block.TopRight.X; x++)
        for (int y = block.BottomLeft.Y; y < block.TopRight.Y; y++)
        {
            var pixel = Pixels[x, y];
            r += pixel.R*pixel.R;
            g += pixel.G*pixel.G;
            b += pixel.B*pixel.B;
            a += pixel.A*pixel.A;
        }

        return new Rgba(
            (int)Math.Sqrt(r / pixelsCount),
            (int)Math.Sqrt(g / pixelsCount),
            (int)Math.Sqrt(b / pixelsCount),
            (int)Math.Sqrt(a / pixelsCount));
    }

    public Rgba GetAverageColorByGeometricMedian(Block block)
    {
        return GeometricMedian.GetGeometricMedian(this, block);
    }

    public Rgba GetAverageColorByGeometricMedian(int left, int bottom, int width, int height)
    {
        return GeometricMedian.GetGeometricMedian(this, left, left+width, bottom, bottom+height);
    }

    // public Rgba GetAverageColor(int left, int bottom, int width, int height)
    // {
    //
    // }

    public void ToImage(string pngPath)
    {
        using var image = new Image<Rgba32>(Width, Height);
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var pixel = Pixels[x, y];
            image[x, Height - y - 1] = new Rgba32((byte)pixel.R, (byte)pixel.G, (byte)pixel.B, (byte)pixel.A);
        }
        image.Save(pngPath, new PngEncoder());
    }

    public void ToImage(string pngPath, Grid grid)
    {
        using var image = new Image<Rgba32>(Width, Height);
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var pixel = Pixels[x, y];
            image[x, Height - y - 1] = new Rgba32((byte)pixel.R, (byte)pixel.G, (byte)pixel.B, (byte)pixel.A);
        }

        var top = 0;
        foreach (var row in grid.Rows)
        {
            var left = 0;
            for (int xx = 0; xx < Width; xx++)
            {
                image[xx, Height - top-1] = new Rgba32(255, 255, 0, 255);
            }
            foreach (var cell in row.Cells)
            {
                left += cell.Width;
                if (left >= Width) continue;
                for (int yy = top; yy < top+row.Height; yy++)
                {
                    image[left, Height - yy-1] = new Rgba32(255, 255, 0, 255);
                }
            }
            top += row.Height;
        }
        image.Save(pngPath, new PngEncoder());
    }

    public int CalculateScore(IEnumerable<Move> moves)
    {
        var canvas = new Canvas(this);
        foreach (var move in moves)
            canvas.Apply(move);
        return canvas.GetScore(this);
    }

    public void MovesToImage(IEnumerable<Move> moves, string filename)
    {
        var canvas = new Canvas(this);
        foreach (var move in moves)
            canvas.Apply(move);
        canvas.ToScreen().ToImage(filename);
    }

    public double DiffTo(Block block, V bottomLeft, int width, int height)
    {
        switch (block)
        {
            case ComplexBlock cb:
                return cb.Children.Sum(c => DiffTo(c, bottomLeft, width, height));
            case AtomicBlock ab:
                var left = Math.Max(ab.Left, bottomLeft.X);
                var right = Math.Min(ab.Right, bottomLeft.X + width);
                var bottom = Math.Max(ab.Bottom, bottomLeft.Y);
                var top = Math.Min(ab.Top, bottomLeft.Y + height);
                if (left >= right || bottom >= top) return 0;
                if (ab is SimpleBlock sb)
                    return DiffTo(new V(left, bottom), new V(right, top), sb.Color);
                var diff = 0.0;
                for (int x = left; x < right ; x++)
                for (int y = bottom; y < top ; y++)
                    diff += ab.ColorAt(x, y).DiffTo(Pixels[x, y]);
                return diff * Alpha;
            default:
                throw new Exception(block.ToString());
        }
    }
}
