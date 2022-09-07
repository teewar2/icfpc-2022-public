using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace lib;

public static class SwapperPreprocessor
{
    public static (Screen problem, int[] rows) Preprocess(Screen problem, int n)
    {
        if (400 % n != 0)
            throw new Exception("(400 % n) != 0");

        var grid = GridBuilder.BuildRegularGrid(problem, n, 40);
        (grid, _) = GridBuilder.OptimizeCellWidths(problem, grid);
        (grid, _) = GridBuilder.OptimizeCellsViaMerge(problem, grid);

        var simplicity = new double[n];
        var indexesSortedBySimplicity = new int[n];
        var bottom = 0;
        var bottoms = new int[n];
        for (int i = 0; i < grid.Rows.Count; i++)
        {
            bottoms[i] = bottom;
            indexesSortedBySimplicity[i] = i;
            simplicity[i] = -grid.Rows[i].Cells.Count;
            bottom += grid.Rows[i].Height;
        }
        Array.Sort(simplicity, indexesSortedBySimplicity);


        var height = grid.Rows[0].Height;
        var result = new Screen(400, 400) { InitialBlocks = problem.InitialBlocks };
        for (int i = 0; i < n; i++)
        {
            var toIndex = i;
            var fromIndex = indexesSortedBySimplicity[i];

            for (int dy = 0; dy < grid.Rows[0].Height; dy++)
            for (int x = 0; x < 400; x++)
            {
                result.Pixels[x, toIndex * height + dy] = problem.Pixels[x, fromIndex * height + dy];
            }
        }

        var rows = Enumerable.Range(0, n).ToArray();
        Array.Sort(indexesSortedBySimplicity, rows);

        return (result, rows);
    }

    public static List<Move> Postprocess(int[] rows, Canvas canvas)
    {
        if (400 % rows.Length != 0)
            throw new Exception("(400 % rows.Length) != 0");
        if (canvas.Blocks.Count != 1)
            throw new Exception("canvas.Blocks.Count != 1");

        var n = rows.Length;
        var block = canvas.Blocks.Single().Value;
        var moves = new List<Move>();
        var height = 400 / rows.Length;
        var blocks = new Block[n];
        for (int i = 0; i < n - 1; i++)
        {
            var hCut = new HCutMove(block.Id, (i + 1) * height);
            var (bottomBlock, topBlock) = canvas.ApplyHCut(hCut);
            moves.Add(hCut);
            block = topBlock;
            blocks[i] = bottomBlock;
        }

        blocks[n - 1] = block;

        var blockIdToPosition = blocks.Select((x, i) => (x, i)).ToDictionary(x => x.x.Id, x => x.i);

        var simpleBlocksToMove = rows.Select(x => blocks[x]).ToArray();

        for (int i = 0; i < simpleBlocksToMove.Length; i++)
        {
            var p1 = blockIdToPosition[simpleBlocksToMove[i].Id];
            var p2 = i;
            if (p1 == p2)
            {
                Console.WriteLine($"SKIP {simpleBlocksToMove[i].Id}");
                continue;
            }

            moves.Add(new SwapMove(blocks[i].Id, simpleBlocksToMove[i].Id));

            blockIdToPosition[blocks[i].Id] = p1;
            blockIdToPosition[simpleBlocksToMove[i].Id] = p2;

            (blocks[p1], blocks[p2]) = (blocks[p2], blocks[p1]);
        }

        return moves;
    }
}
