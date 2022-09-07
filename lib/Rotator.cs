using System;
using System.Collections.Generic;
using System.Linq;

namespace lib;

public static class Rotator
{
    public const int ScreenSize = 400;

    public static Screen Rotate(Screen problem, int orientation)
    {
        if (orientation >= 4)
        {
            problem = FlipUpsideDown(problem);
            orientation -= 4;
        }

        for (int i = 0; i < orientation; i++)
            problem = RotateCCW(problem);

        return problem;
    }

    public static Screen RotateBack(Screen problem, int orientation)
    {
        var rotations = (4 - (orientation & ~4)) % 4;

        for (int i = 0; i < rotations; ++i)
            problem = RotateCCW(problem);

        if (orientation >= 4)
            problem = FlipUpsideDown(problem);

        return problem;
    }

    public static List<Move> Rotate(Screen problem, List<Move> moves, int orientation)
    {
        if (orientation >= 4)
        {
            moves = FlipUpsideDown(problem, moves);
            problem = FlipUpsideDown(problem);
            orientation -= 4;
        }

        for (int i = 0; i < orientation; i++)
        {
            moves = RotateCCW(problem, moves);
            problem = RotateCCW(problem);
        }

        return moves;
    }

    public static List<Move> RotateBack(Screen problem, List<Move> moves, int orientation)
    {
        var rotations = (4 - (orientation & ~4)) % 4;

        for (int i = 0; i < rotations; ++i)
        {
            moves = RotateCCW(problem, moves);
            problem = RotateCCW(problem);
        }

        if (orientation >= 4)
           moves = FlipUpsideDown(problem, moves);

        return moves;
    }

    public static Screen FlipUpsideDown(Screen problem)
    {
        var pixels = new Rgba[problem.Width, problem.Height];
        for (int x = 0; x < problem.Width; x++)
        for (int y = 0; y < problem.Height; y++)
            pixels[x, y] = problem.Pixels[x, ScreenSize - y - 1];

        var sourcePng = problem.InitialBlocks.OfType<PngBlock>().Select(x => x.SourcePng).SingleOrDefault();
        if (sourcePng != null)
        {
            var pngPixels = new Rgba[ScreenSize, ScreenSize];
            for (int x = 0; x < sourcePng.GetLength(0); x++)
            for (int y = 0; y < sourcePng.GetLength(1); y++)
                pngPixels[x, y] = sourcePng[x, ScreenSize - y - 1];
            sourcePng = pngPixels;
        }

        return new Screen(pixels)
        {
            InitialBlocks = problem.InitialBlocks.Select(
                b =>
                {
                    var v1 = FlipUpsideDown(b.BottomLeft);
                    var v2 = FlipUpsideDown(b.TopRight);
                    b = b with
                    {
                        BottomLeft = new V(v1.X, v2.Y),
                        TopRight = new V(v2.X, v1.Y),
                    };
                    if (b is PngBlock pngBlock)
                    {
                        if (pngBlock.BottomLeft != V.Zero)
                            throw new Exception("pngBlock.BottomLeft != V.Zero");
                        b = pngBlock with
                        {
                            SourcePng = sourcePng!,
                        };
                    }

                    return b;
                }).ToArray()
        };
    }

    public static Screen RotateCCW(Screen problem)
    {
        var pixels = new Rgba[problem.Width, problem.Height];
        for (int x = 0; x < problem.Width; x++)
        for (int y = 0; y < problem.Height; y++)
        {
            pixels[ScreenSize - y - 1, x] = problem.Pixels[x, y];
        }

        var sourcePng = problem.InitialBlocks.OfType<PngBlock>().Select(x => x.SourcePng).SingleOrDefault();
        if (sourcePng != null)
        {
            var pngPixels = new Rgba[ScreenSize, ScreenSize];
            for (int x = 0; x < sourcePng.GetLength(0); x++)
            for (int y = 0; y < sourcePng.GetLength(1); y++)
                pngPixels[ScreenSize - y - 1, x] = sourcePng[x, y];
            sourcePng = pngPixels;
        }


        return new Screen(pixels)
        {
            InitialBlocks = problem.InitialBlocks.Select(
                b =>
                {
                    var v1 = RotateCCW(b.BottomLeft);
                    var v2 = RotateCCW(b.TopRight);
                    b = b with
                    {
                        BottomLeft = new V(v2.X, v1.Y),
                        TopRight = new V(v1.X, v2.Y),
                    };
                    if (b is PngBlock pngBlock)
                    {
                        if (pngBlock.BottomLeft != V.Zero)
                            throw new Exception("pngBlock.BottomLeft != V.Zero");
                        b = pngBlock with
                        {
                            SourcePng = sourcePng!,
                        };
                    }
                    return b;
                }).ToArray()
        };
    }

    public static List<Move> FlipUpsideDown(Screen problem, List<Move> moves)
    {
        var canvas = new Canvas(problem);

        var ids = new Dictionary<string, string>();
        foreach (var blockId in canvas.Blocks.Keys)
            ids[blockId] = blockId;

        var result = new List<Move>();
        foreach (var move in moves)
        {
            switch (move)
            {
                case ColorMove colorMove:
                    canvas.ApplyColor(colorMove);
                    result.Add(Rotate(colorMove, ids));
                    break;
                case MergeMove mergeMove:
                    var complexBlock = canvas.ApplyMerge(mergeMove);
                    result.Add(Rotate(mergeMove, complexBlock, ids));
                    break;
                case SwapMove swapMove:
                    canvas.ApplySwap(swapMove);
                    result.Add(Rotate(swapMove, ids));
                    break;
                case HCutMove hCutMove:
                    var hCutResult = canvas.ApplyHCut(hCutMove);
                    result.Add(FlipUpsideDown(hCutMove, hCutResult, ids));
                    break;
                case VCutMove vCutMove:
                    var vCutResult = canvas.ApplyVCut(vCutMove);
                    result.Add(FlipUpsideDown(vCutMove, vCutResult, ids));
                    break;
                case PCutMove pCutMove:
                    var pCutResult = canvas.ApplyPCut(pCutMove);
                    result.Add(FlipUpsideDown(pCutMove, pCutResult, ids));
                    break;
                case NopMove:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(move));
            }
        }

        return result;
    }

    public static List<Move> RotateCCW(Screen problem, List<Move> moves)
    {
        var canvas = new Canvas(problem);

        var ids = new Dictionary<string, string>();
        foreach (var blockId in canvas.Blocks.Keys)
            ids[blockId] = blockId;

        var result = new List<Move>();
        foreach (var move in moves)
        {
            switch (move)
            {
                case ColorMove colorMove:
                    canvas.ApplyColor(colorMove);
                    result.Add(Rotate(colorMove, ids));
                    break;
                case MergeMove mergeMove:
                    var complexBlock = canvas.ApplyMerge(mergeMove);
                    result.Add(Rotate(mergeMove, complexBlock, ids));
                    break;
                case SwapMove swapMove:
                    canvas.ApplySwap(swapMove);
                    result.Add(Rotate(swapMove, ids));
                    break;
                case HCutMove hCutMove:
                    var hCutResult = canvas.ApplyHCut(hCutMove);
                    result.Add(RotateCCW(hCutMove, hCutResult, ids));
                    break;
                case VCutMove vCutMove:
                    var vCutResult = canvas.ApplyVCut(vCutMove);
                    result.Add(RotateCCW(vCutMove, vCutResult, ids));
                    break;
                case PCutMove pCutMove:
                    var pCutResult = canvas.ApplyPCut(pCutMove);
                    result.Add(RotateCCW(pCutMove, pCutResult, ids));
                    break;
                case NopMove:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(move));
            }
        }

        return result;
    }

    public static HCutMove FlipUpsideDown(HCutMove move, (Block bottomBlock, Block topBlock) result, Dictionary<string, string> ids)
    {
        var parentId = ids[move.BlockId];
        ids[result.bottomBlock.Id] = parentId + ".1";
        ids[result.topBlock.Id] = parentId + ".0";
        return new HCutMove(parentId, ScreenSize - move.LineNumber);
    }

    public static VCutMove RotateCCW(HCutMove move, (Block bottomBlock, Block topBlock) result, Dictionary<string, string> ids)
    {
        var parentId = ids[move.BlockId];
        ids[result.bottomBlock.Id] = parentId + ".1";
        ids[result.topBlock.Id] = parentId + ".0";
        return new VCutMove(parentId, RotateCCW(new V(0, move.LineNumber)).X);
    }

    public static VCutMove FlipUpsideDown(VCutMove move, (Block leftBlock, Block rightBlock) result, Dictionary<string, string> ids)
    {
        var parentId = ids[move.BlockId];
        ids[result.leftBlock.Id] = parentId + ".0";
        ids[result.rightBlock.Id] = parentId + ".1";
        return new VCutMove(parentId, move.LineNumber);
    }

    public static HCutMove RotateCCW(VCutMove move, (Block leftBlock, Block rightBlock) result, Dictionary<string, string> ids)
    {
        var parentId = ids[move.BlockId];
        ids[result.leftBlock.Id] = parentId + ".0";
        ids[result.rightBlock.Id] = parentId + ".1";
        return new HCutMove(parentId, RotateCCW(new V(move.LineNumber, 0)).Y);
    }

    public static PCutMove FlipUpsideDown(PCutMove move, (Block bottomLeftBlock, Block bottomRightBlock, Block topRightBlock, Block topLeftBlock) result, Dictionary<string, string> ids)
    {
        var parentId = ids[move.BlockId];
        ids[result.bottomLeftBlock.Id] = parentId + ".3";
        ids[result.bottomRightBlock.Id] = parentId + ".2";
        ids[result.topRightBlock.Id] = parentId + ".1";
        ids[result.topLeftBlock.Id] = parentId + ".0";
        return new PCutMove(parentId, FlipUpsideDown(move.Point));
    }

    public static PCutMove RotateCCW(PCutMove move, (Block bottomLeftBlock, Block bottomRightBlock, Block topRightBlock, Block topLeftBlock) result, Dictionary<string, string> ids)
    {
        var parentId = ids[move.BlockId];
        ids[result.bottomLeftBlock.Id] = parentId + ".1";
        ids[result.bottomRightBlock.Id] = parentId + ".2";
        ids[result.topRightBlock.Id] = parentId + ".3";
        ids[result.topLeftBlock.Id] = parentId + ".0";
        return new PCutMove(parentId, RotateCCW(move.Point));
    }

    public static ColorMove Rotate(ColorMove move, Dictionary<string, string> ids)
    {
        var parentId = ids[move.BlockId];
        return new ColorMove(parentId, move.Color);
    }

    public static MergeMove Rotate(MergeMove move, ComplexBlock result, Dictionary<string, string> ids)
    {
        var parentId1 = ids[move.Block1Id];
        var parentId2 = ids[move.Block2Id];
        ids[result.Id] = result.Id;
        return new MergeMove(parentId1, parentId2);
    }

    public static SwapMove Rotate(SwapMove move, Dictionary<string, string> ids)
    {
        var parentId1 = ids[move.Block1Id];
        var parentId2 = ids[move.Block2Id];
        return new SwapMove(parentId1, parentId2);
    }

    public static V FlipUpsideDown(V v)
    {
        return new V(v.X, ScreenSize - v.Y);
    }

    public static V RotateCCW(V v)
    {
        return new V(ScreenSize - v.Y, v.X);
    }
}
