using System;
using System.Collections.Generic;

namespace lib;

enum InstructionType
{
    ColorInstructionType,
    HorizontalCutInstructionType,
    VerticalCutInstructionType,
    PointCutInstructionType,
    SwapInstructionType,
    MergeInstructionType,
}

public class RandomInstructionGenerator
{
    private readonly Random random;

    private readonly InstructionType[] instructionTypesDistribution =
    {
        InstructionType.ColorInstructionType,
        InstructionType.ColorInstructionType,
        InstructionType.ColorInstructionType,
        InstructionType.ColorInstructionType,
        InstructionType.HorizontalCutInstructionType,
        InstructionType.VerticalCutInstructionType,
        InstructionType.PointCutInstructionType,
        InstructionType.SwapInstructionType,
        InstructionType.MergeInstructionType,
    };

    public RandomInstructionGenerator(int? seed)
    {
        var localRandom = new Random();
        seed ??= localRandom.Next();
        Console.WriteLine($"Seed: {seed}");
        random = new(seed.Value);
    }

    public Move GenerateRandomInstruction(Canvas state)
    {
        var instructionType = instructionTypesDistribution.Sample(random);

        if (instructionType == InstructionType.ColorInstructionType)
        {
            var block = state.Blocks.Values.Sample(random);
            var color = GenerateRandomColor();

            return new ColorMove(block.Id, color);
        }

        if (instructionType == InstructionType.HorizontalCutInstructionType)
        {
            var block = state.Blocks.Values.Sample(random);
            var min = block.BottomLeft.Y + 1;
            var max = block.TopRight.Y - 1;

            if (max - min <= 1)
            {
                return new NopMove();
            }

            var position = (int) Math.Floor(random.NextDouble() * (max - min) + min);

            return new HCutMove(block.Id, position);
        }

        if (instructionType == InstructionType.VerticalCutInstructionType)
        {
            var block = state.Blocks.Values.Sample(random);
            var min = block.BottomLeft.X + 1;
            var max = block.TopRight.X - 1;

            if (max - min <= 1)
            {
                return new NopMove();
            }

            var position = (int) Math.Floor(random.NextDouble() * (max - min) + min);

            return new VCutMove(block.Id, position);
        }

        if (instructionType == InstructionType.PointCutInstructionType)
        {
            var block = state.Blocks.Values.Sample(random);

            var xMin = block.BottomLeft.X + 1;
            var xMax = block.TopRight.X - 1;

            var yMin = block.BottomLeft.Y + 1;
            var yMax = block.TopRight.Y - 1;

            if (xMax - xMin <= 1 || yMax - yMin <= 1)
            {
                return new NopMove();
            }

            var xPosition = (int) Math.Floor(random.NextDouble() * (xMax - xMin) + xMin);
            var yPosition = (int) Math.Floor(random.NextDouble() * (yMax - yMin) + yMin);

            return new PCutMove(block.Id, new(xPosition, yPosition));
        }

        if (instructionType == InstructionType.SwapInstructionType)
        {
            var swapPairs = new List<(string, string)>();

            foreach (var (blockId1, block1) in state.Blocks)
            foreach (var (blockId2, block2) in state.Blocks)
            {
                if (blockId1 == blockId2) continue;
                if (block1.Size != block2.Size) continue;
                swapPairs.Add((blockId1, blockId2));
            }

            if (swapPairs.Count == 0)
            {
                return new NopMove();
            }

            var pair = swapPairs.Sample(random);
            return new SwapMove(pair.Item1, pair.Item2);
        }

        if (instructionType == InstructionType.MergeInstructionType)
        {
            var mergePairs = new List<(string, string)>();
            foreach (var (blockId1, block1) in state.Blocks)
            {
                foreach (var (blockId2, block2) in state.Blocks)
                {
                    if (blockId1 == blockId2) continue;
                    var bottomToTop =
                        (block1.BottomLeft.Y == block2.TopRight.Y ||
                            block1.TopRight.Y == block2.BottomLeft.Y) &&
                        block1.BottomLeft.X == block2.BottomLeft.X &&
                        block1.TopRight.X == block2.TopRight.X;

                    var leftToRight =
                        (block1.BottomLeft.X == block2.TopRight.X ||
                            block1.TopRight.X == block2.BottomLeft.X) &&
                        block1.BottomLeft.Y == block2.BottomLeft.Y &&
                        block1.TopRight.Y == block2.TopRight.Y;

                    var mergable = bottomToTop || leftToRight;
                    if (mergable)
                        mergePairs.Add((blockId1, blockId2));
                }

                if (mergePairs.Count == 0)
                {
                    return new NopMove();
                }

                var pair = mergePairs.Sample(random);

                return new MergeMove(pair.Item1, pair.Item2);
            }
        }

        return new NopMove();
    }

    public Rgba GenerateRandomColor()
    {
        return new(random.Next(255), random.Next(255), random.Next(255), random.Next(255));
    }
}
