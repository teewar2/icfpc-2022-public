using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Algorithms;

public class RandomSearchAlgorithm
{
    private int moveSequencesCount = 1400;
    private int depth = 4;
    private int movesWithoutImprovements = 200;
    private RandomInstructionGenerator instructionGenerator;

    public RandomSearchAlgorithm(int? seed = null)
    {
        instructionGenerator = new(seed);
    }

    public IEnumerable<Move> Solve(Screen problem)
    {
        var canvas = new Canvas(problem);
        var bestScore = canvas.GetScore(problem);
        var movesToEscape = movesWithoutImprovements;

        var improvingMoves = new List<Move>();
        // var currentMoves = new List<Move>();

        while (movesToEscape > 0)
        {
            movesToEscape--;

            var randomMove = GetRandomMove(canvas, problem);

            if (randomMove.Score < bestScore)
            {
                // Console.WriteLine($"Improvement before {movesToEscape} moves to escape");
                bestScore = randomMove.Score;
                canvas = randomMove.Canvas;
                improvingMoves.AddRange(randomMove.Moves);
                movesToEscape = movesWithoutImprovements;
            }
        }

        return improvingMoves;
    }

    private SolutionElement GetRandomMove(Canvas canvas, Screen screen)
    {
        var bestScore = int.MaxValue;
        var bestMoveSequence = Enumerable.Repeat(new NopMove(), depth).ToList<Move>();
        var bestCanvas = canvas;

        for (int i = 0; i < moveSequencesCount; i++)
        {
            var copy = canvas.Copy();
            var moves = GetAndApplyRandomMoveSequence(copy);
            var score = copy.GetScore(screen);

            if (score < bestScore)
            {
                bestScore = score;
                bestMoveSequence = moves;
                bestCanvas = copy;
            }
        }

        return new SolutionElement(bestScore, bestMoveSequence, bestCanvas);
    }

    private List<Move> GetAndApplyRandomMoveSequence(Canvas canvas)
    {
        var moves = new List<Move>();
        for (var i = 0; i < depth; i++)
        {
            var randomMove = instructionGenerator.GenerateRandomInstruction(canvas);
            moves.Add(randomMove);
            canvas.Apply(randomMove);
        }

        return moves;
    }

    public record SolutionElement(int Score, List<Move> Moves, Canvas Canvas);
}
