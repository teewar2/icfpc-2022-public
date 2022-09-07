using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using lib;
using lib.Algorithms;
using lib.db;
using NUnit.Framework;
using TextCopy;

namespace tests;

public class MergerTests
{
    [Test]
    public async Task Test([Range(26, 35)]int problemId)
    {
        var originalTaskByNewTaskId = new Dictionary<int, int>()
        {
            { 26, 5 },
            { 27, 2 },
            { 28, 10 },
            { 29, 18 },
            { 30, 11 },
            { 31, 24 },
            { 32, 9 },
            { 33, 15 },
            { 34, 7 },
            { 35, 25 },
        };
        var screen = Screen.LoadProblem(problemId);
        var solution = await SolutionRepo.GetBestSolutionByProblemId(originalTaskByNewTaskId[problemId]);
        var originalMoves = Moves.Parse(solution!.Solution);
        var newSolution = screen.MergeAllAndApplyExistedSolution(originalMoves);
        var newScore = screen.GetScore(newSolution);
        Console.WriteLine(newScore);
        var bestSolution = await SolutionRepo.GetBestSolutionByProblemId(problemId);
        var isImproved = bestSolution!.ScoreEstimated > newScore;
        Console.WriteLine($"{bestSolution!.ScoreEstimated} → {newScore}" + (isImproved ? " IMPROVEMENT!!!!" : ""));
        var newSolutionAsText = newSolution.StrJoin("\n");
        await SolutionRepo.SubmitAsync(new ContestSolution(problemId, newScore, newSolutionAsText, new SolverMeta(){ Description = "MergeAll" }, solution.SolverId ));
        await ClipboardService.SetTextAsync(newSolutionAsText);
    }
}
