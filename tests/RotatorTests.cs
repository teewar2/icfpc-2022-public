using System;
using System.IO;
using lib;
using NUnit.Framework;

namespace tests;

[TestFixture]
public class RotatorTests
{
    [Test]
    public void TestRotateProblem()
    {
        var problem = Screen.LoadProblem(4);
        for (int i = 0; i < 8; i++)
        {
            var rotated = Rotator.Rotate(problem, i);
            rotated.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), $"rotated{i}.png"));
            var rotatedBack = Rotator.RotateBack(rotated, i);
            rotatedBack.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), $"rotated{i}-back.png"));
        }
    }

    [Test]
    public void TestRotateMoves()
    {
        var moves = Moves.Parse(File.ReadAllText(Path.Combine(FileHelper.FindDirectoryUpwards("hand-solutions"), "problem-4-18418.txt")));
        var problem = Screen.LoadProblem(4);
        Console.Out.WriteLine(problem.GetScore(moves));

        for (int i = 0; i < 8; i++)
        {
            var rotated = Rotator.Rotate(problem, i);
            var rotatedMoves = Rotator.Rotate(problem, moves, i);
            Console.Out.WriteLine($"{i}={rotated.GetScore(rotatedMoves)}");

            var rotatedBack = Rotator.RotateBack(rotated, i);
            var rotatedMovesBack = Rotator.RotateBack(rotatedBack, rotatedMoves, i);
            Console.Out.WriteLine($"{i}={rotatedBack.GetScore(rotatedMovesBack)}");

            // rotated.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), $"rotated{i}.png"));
            // var rotatedBack = Rotator.RotateBack(rotated, i);
            // rotatedBack.ToImage(Path.Combine(FileHelper.FindDirectoryUpwards("tests"), $"rotated{i}-back.png"));
        }
    }
}
