using System;
using System.Collections.Generic;
using System.Linq;
using lib.Algorithms;

namespace lib.Enhancers;

public class ColorPicker : ISolutionEnhancer
{
    public List<Move> Enhance(Screen problem, List<Move> moves)
    {
        var originalMoves = moves;
        var originalScore = problem.CalculateScore(originalMoves);

        moves = EnrichMovesWithColors(problem, moves);

        var map = BuildBlocksMap(problem, moves);
        moves = RemoveObsoleteColorMoves(problem, moves, map);

        var graph = BuildColorGraph(moves, map);

        moves = MakeUnavoidableColorMoves(problem, moves, map, graph);

        moves = MakeApproximateColorMoves(problem, moves, map);
        moves = RemoveDuplicateColorMoves(problem, moves, map);

        if (problem.CalculateScore(moves) < originalScore)
            return moves;

        return originalMoves;
    }

    private static List<Move> OptimizeColorMove(
        Screen problem,
        List<Move> moves,
        Dictionary<string, (V bottomLeft, V topRight)[]> map,
        Dictionary<string, List<string>> graph,
        string blockId
    )
    {
        var noDups = RemoveDuplicateColorMoves(problem, moves, map);
        var originalScore = problem.CalculateScore(noDups);

        var canvas = new Canvas(problem);
        foreach (var move in moves)
        {
            canvas.Apply(move);
        }
        return moves;
    }

    private static List<Move> RemoveDuplicateColorMoves(Screen problem, List<Move> moves, Dictionary<string, (V bottomLeft, V topRight)[]> map)
    {
        var canvas = new Canvas(problem);
        var result = new List<Move>();
        foreach (var move in moves)
        {
            if (move is ColorMove cm)
            {
                if (canvas.Blocks[cm.BlockId] is SimpleBlock)
                {
                    var intermediate = canvas.ToScreen();
                    var need = false;
                    foreach (var ((x0, y0), (x1, y1)) in map[cm.BlockId])
                    {
                        for (int x = x0; x < x1; x++)
                        {
                            for (int y = y0; y < y1; y++)
                            {
                                if (intermediate.Pixels[x, y] != cm.Color)
                                {
                                    need = true;
                                    break;
                                }
                            }
                            if (need) break;
                        }
                        if (need) break;
                    }

                    if (!need)
                        continue;
                }
            }

            result.Add(move);
            canvas.Apply(move);
        }

        return result;
    }

    private static List<Move> RemoveObsoleteColorMoves(Screen problem, List<Move> moves, Dictionary<string, (V bottomLeft, V topRight)[]> map)
    {
        var canvas = new Canvas(problem);
        var result = new List<Move>();
        foreach (var move in moves)
        {
            if (move is ColorMove cm)
            {
                if (!map.ContainsKey(cm.BlockId))
                {
                    // if (canvas.Blocks[cm.BlockId] is not SimpleBlock)
                    //     throw new Exception("Block is not simple");
                    continue;
                }
            }

            result.Add(move);
            canvas.Apply(move);
        }

        return result;
    }

    private static List<Move> MakeUnavoidableColorMoves(Screen problem, List<Move> moves, Dictionary<string, (V bottomLeft, V topRight)[]> map, Dictionary<string, List<string>> graph)
    {
        var canvas = new Canvas(problem);
        var result = new List<Move>();
        foreach (var move in moves)
        {
            if (move is ColorMove cm)
            {
                if (!graph.ContainsKey(cm.BlockId))
                {
                    if (canvas.Blocks[cm.BlockId] is ComplexBlock)
                    {
                        var pixels = new List<Rgba>();
                        foreach (var ((x0, y0), (x1, y1)) in map[cm.BlockId])
                        {
                            for (int x = x0; x < x1; x++)
                            for (int y = y0; y < y1; y++)
                                pixels.Add(problem.Pixels[x, y]);
                        }

                        var cmm = cm with { Color = GeometricMedian.GetGeometricMedian(pixels) };
                        result.Add(cmm);
                        canvas.Apply(cmm);
                        continue;
                    }
                }
            }

            result.Add(move);
            canvas.Apply(move);
        }

        return result;
    }

    private static List<Move> MakeApproximateColorMoves(Screen problem, List<Move> moves, Dictionary<string, (V bottomLeft, V topRight)[]> map)
    {
        var canvas = new Canvas(problem);
        var result = new List<Move>();
        foreach (var move in moves)
        {
            if (move is ColorMove cm)
            {
                if (canvas.Blocks[cm.BlockId] is SimpleBlock)
                {
                    var pixels = new List<Rgba>();
                    foreach (var ((x0, y0), (x1, y1)) in map[cm.BlockId])
                    {
                        for (int x = x0; x < x1; x++)
                        for (int y = y0; y < y1; y++)
                            pixels.Add(problem.Pixels[x, y]);
                    }

                    var cmm = cm with { Color = GeometricMedian.GetGeometricMedian(pixels) };
                    result.Add(cmm);
                    canvas.Apply(cmm);
                    continue;
                }
            }

            result.Add(move);
            canvas.Apply(move);
        }

        return result;
    }

    private static Dictionary<string, List<string>> BuildColorGraph(List<Move> moves, Dictionary<string, (V bottomLeft, V topRight)[]> map)
    {
        var graph = new Dictionary<string, List<string>>();
        var colorBlocks = moves.OfType<ColorMove>().Select(x => x.BlockId).Reverse().ToArray();
        for (int i = 0; i < colorBlocks.Length; i++)
        {
            var b1 = map.GetOrDefault(colorBlocks[i], null!);
            if (b1 == null!)
                continue;

            for (int j = i + 1; j < colorBlocks.Length; j++)
            {
                var b2 = map.GetOrDefault(colorBlocks[j], null!);
                if (b2 == null!)
                    continue;
                if (!b1.Except(b2).Any())
                {
                    var list = graph.GetOrCreate(colorBlocks[i], _ => new List<string>());
                    list.Add(colorBlocks[j]);
                    if (list.Count == 1)
                        break;
                }

                if (b1.Intersect(b2).Any())
                    graph.GetOrCreate(colorBlocks[i], _ => new List<string>()).Add(colorBlocks[j]);
            }
        }

        return graph;
    }

    private static Dictionary<string, (V bottomLeft, V topRight)[]> BuildBlocksMap(Screen problem, List<Move> moves)
    {
        var colorIndexes = moves.Select((move, i) => new { move, i }).Where(x => x.move is ColorMove).Select(x => x.i).ToArray();
        var map = new Dictionary<string, (V bottomLeft, V topRight)[]>();
        var canvas = new Canvas(problem);
        var empty = new Rgba(255, 255, 255, 255);
        var filled = new Rgba(0, 0, 0, 0);
        foreach (var colorIndex in colorIndexes)
        {
            var copy = canvas.Copy();
            ApplyRange(copy, moves, 0, colorIndex - 1, (_, m) =>
            {
                if (m is ColorMove cm)
                    return cm with { Color = empty };
                return m;
            });
            var colorMove = (ColorMove)moves[colorIndex];
            copy.Apply(colorMove with { Color = filled });
            ApplyRange(copy, moves, colorIndex + 1, moves.Count - 1, (c, m) =>
            {
                if (m is ColorMove cm)
                {
                    if (c.Blocks[cm.BlockId] is SimpleBlock)
                        return null;
                    return cm with { Color = empty };
                }

                return m;
            });
            var blocks = copy.Flatten().OfType<SimpleBlock>().Where(x => x.Color == filled).Select(x => (x.BottomLeft, x.TopRight)).ToArray();
            if (blocks.Any())
                map[colorMove.BlockId] = blocks;
        }

        return map;
    }

    private static void ApplyRange(Canvas canvas, List<Move> moves, int start, int end, Func<Canvas, Move, Move?> changeMove)
    {
        for (var i = start; i <= end; i++)
        {
            var move = moves[i];
            var changedMove = changeMove(canvas, move);
            if (changedMove != null)
                canvas.Apply(changedMove);
        }
    }

    private static List<Move> EnrichMovesWithColors(Screen problem, List<Move> moves)
    {
        var canvas = new Canvas(problem);
        var pureMoves = new List<Move>();

        var first = new ColorMove(canvas.Blocks.Single().Key, new Rgba(0, 0, 0, 0));
        pureMoves.Add(first);
        canvas.Apply(first);

        foreach (var move in moves)
        {
            if (move is ColorMove colorMove)
            {
                if (canvas.Blocks[colorMove.BlockId] is SimpleBlock)
                    continue;
                var newColorMove = colorMove with { Color = new Rgba(0, 0, 0, 0) };
                pureMoves.Add(newColorMove);
                canvas.Apply(newColorMove);
            }
            else if (move is VCutMove vCutMove)
            {
                var (leftBlock, rightBlock) = canvas.ApplyVCut(vCutMove);
                pureMoves.Add(move);
                if (leftBlock is SimpleBlock)
                {
                    var childMove = new ColorMove(leftBlock.Id, new Rgba(0, 0, 0, 0));
                    pureMoves.Add(childMove);
                    canvas.Apply(childMove);
                }

                if (rightBlock is SimpleBlock)
                {
                    var childMove = new ColorMove(rightBlock.Id, new Rgba(0, 0, 0, 0));
                    pureMoves.Add(childMove);
                    canvas.Apply(childMove);
                }
            }
            else if (move is HCutMove hCutMove)
            {
                var (bottomBlock, topBlock) = canvas.ApplyHCut(hCutMove);
                pureMoves.Add(move);
                if (bottomBlock is SimpleBlock)
                {
                    var childMove = new ColorMove(bottomBlock.Id, new Rgba(0, 0, 0, 0));
                    pureMoves.Add(childMove);
                    canvas.Apply(childMove);
                }

                if (topBlock is SimpleBlock)
                {
                    var childMove = new ColorMove(topBlock.Id, new Rgba(0, 0, 0, 0));
                    pureMoves.Add(childMove);
                    canvas.Apply(childMove);
                }
            }
            else if (move is PCutMove pCutMove)
            {
                var (bottomLeftBlock, bottomRightBlock, topRightBlock, topLeftBlock) = canvas.ApplyPCut(pCutMove);
                pureMoves.Add(move);
                if (bottomLeftBlock is SimpleBlock)
                {
                    var childMove = new ColorMove(bottomLeftBlock.Id, new Rgba(0, 0, 0, 0));
                    pureMoves.Add(childMove);
                    canvas.Apply(childMove);
                }

                if (bottomRightBlock is SimpleBlock)
                {
                    var childMove = new ColorMove(bottomRightBlock.Id, new Rgba(0, 0, 0, 0));
                    pureMoves.Add(childMove);
                    canvas.Apply(childMove);
                }

                if (topRightBlock is SimpleBlock)
                {
                    var childMove = new ColorMove(topRightBlock.Id, new Rgba(0, 0, 0, 0));
                    pureMoves.Add(childMove);
                    canvas.Apply(childMove);
                }

                if (topLeftBlock is SimpleBlock)
                {
                    var childMove = new ColorMove(topLeftBlock.Id, new Rgba(0, 0, 0, 0));
                    pureMoves.Add(childMove);
                    canvas.Apply(childMove);
                }
            }
            else if (move is MergeMove or SwapMove)
            {
                pureMoves.Add(move);
                canvas.Apply(move);
            }
        }

        return pureMoves;
    }
}
