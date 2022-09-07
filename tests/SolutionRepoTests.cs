using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using lib;
using lib.Algorithms;
using lib.api;
using lib.db;
using Newtonsoft.Json;
using NUnit.Framework;

namespace tests;

public class SolutionRepoTests
{
    [Test]
    public void SubmitProblemTest([Range(1, 25)] int problemId)
    {
        try
        {
            var screen = Screen.LoadProblem(problemId);
            var algorithm = new SimpleAlgorithm();

            var (moves, score) = algorithm.Solve(screen);

            var commands = string.Join('\n', moves.Select(m => m.ToString()));

            var solution = new ContestSolution(problemId, (long) score,
                commands, new SolverMeta(), nameof(SimpleAlgorithm));
            SolutionRepo.Submit(solution);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Test]
    public void GetBestScoreByProblemIdTest()
    {
        var ans = SolutionRepo.GetBestScoreByProblemId().GetAwaiter().GetResult();
        Console.WriteLine(string.Join(" ", ans));
    }

    [Test]
    public void GetSolutionByIdAndScoreTest()
    {
        var scoresById = SolutionRepo.GetBestScoreByProblemId().GetAwaiter().GetResult();
        foreach (var (problemId, score) in scoresById)
        {
            var solution = SolutionRepo.GetSolutionByProblemIdAndScore(problemId, score).GetAwaiter().GetResult();
            Console.WriteLine(solution);
        }
    }

    [Test]
    public void SubmitManualSolutions()
    {
        var api = new Api();
        var handsDirectory = FileHelper.FindDirectoryUpwards("hand-solutions");
        var filenames = Directory.GetFiles(handsDirectory, "*.txt");
        foreach (var filename in filenames)
        {
            var nameParts = filename.Split('-');
            if (!nameParts[2].Contains("problem"))
                continue;
            var problemId = int.Parse(nameParts[3]);
            var program = File.ReadAllText(filename);
            var moves = Moves.Parse(program);
            var screen = Screen.LoadProblem(problemId);
            var canvas = new Canvas(screen);
            foreach (var move in moves)
            {
                canvas.Apply(move);
            }

            // api.PostSolution(int.Parse(nameParts[3]), File.ReadAllText(filename));
            var score = canvas.GetScore(screen);
            SolutionRepo.Submit(new ContestSolution(problemId, score, program, new SolverMeta(), "manual"));
        }
        var scoresById = SolutionRepo.GetBestScoreByProblemId().GetAwaiter().GetResult();
        foreach (var (problemId, score) in scoresById)
        {
            var solution = SolutionRepo.GetSolutionByProblemIdAndScore(problemId, score).GetAwaiter().GetResult();
            Console.WriteLine(solution);
        }
    }

    [Test]
    public void METHOD()
    {
        var problemId = 3;
        var solvers = SolutionRepo.GetAllSolvers(problemId).GetAwaiter().GetResult();
        foreach (var solver in solvers)
        {
            if (solver.EndsWith("-enchanced"))
                continue;
            var sol = SolutionRepo.GetBestSolutionBySolverId(problemId, solver).GetAwaiter().GetResult();
            Console.WriteLine(sol == null ? "sol is null" : $"{sol.SolverId} - {sol.ScoreEstimated}");
        }
    }

    [Test]
    [Explicit]
    public void SaveBestSolution()
    {
        var path = FileHelper.FindDirectoryUpwards("best-solutions");
        var prIds = ScreenRepo.GetProblemIds();
        foreach (var problemId in prIds)
        {
            var sol= SolutionRepo.GetBestSolutionByProblemId(problemId).GetAwaiter().GetResult();
            if (sol == null)
                continue;
            File.WriteAllText(Path.Combine(path, $"sol-{problemId}-{sol.SolverId}-{sol.ScoreEstimated}.txt"),sol.Solution);
            File.WriteAllText(Path.Combine(path, $"sol-{problemId}-{sol.SolverId}-meta.txt"),sol.SolverMeta.ToString());
        }
    }

    [Test]
    [Explicit]
    public void SaveSolutionsWithMeta()
    {
        var path = FileHelper.FindDirectoryUpwards("solutions-with-meta");
        for (int problemId = 1; problemId <= 40; problemId++)
        {
            var problemPath = Path.Combine(path, $"problem{problemId}");
            if (!Directory.Exists(problemPath))
                continue;

            var metas = Directory.GetFiles(problemPath, "meta*");
            if (metas.Length == 0)
                continue;

            var best = metas.OrderBy(x =>
            {
                var name = Path.GetFileNameWithoutExtension(x);
                var split = name.Split("-", 3);
                return int.Parse(split[1]);
            }).First();
            var sol = File.ReadAllText(best).FromJson<SolverMeta>();
            if (best.Contains("enchanced") && !string.IsNullOrEmpty(sol.Previous_SolverName))
            {
                best = Path.Combine(path, $"problem{problemId}", $"meta-{sol.Previous_Score}-{sol.Previous_SolverName}.txt");
                sol = File.ReadAllText(best).FromJson<SolverMeta>();
            }

            if (!best.Contains("GridGuidedPainter"))
                continue;

            if (sol.Description?.Contains("20*20") == true)
                continue;
            if (sol.Description?.Contains("40*40") == true)
                continue;
            if (sol.Description?.Contains("19*19") == true)
                continue;
            if (sol.Description?.Contains("17*17") == true)
                continue;
            if (sol.Description?.Contains("MergeAll") == true)
                continue;

            // if (sol.Description?.Contains("17*17") != true)
            //     continue;


            Console.Out.WriteLine($"problem {problemId}, {Path.GetFileNameWithoutExtension(best)}, sol: {sol}");
        }

        // var path = FileHelper.FindDirectoryUpwards("solutions-with-meta");
        // var prIds = ScreenRepo.GetProblemIds();
        // foreach (var problemId in prIds)
        // {
        //     var sols = SolutionRepo.GetSolutionsByProblemId(problemId).GetAwaiter().GetResult();
        //     Directory.CreateDirectory(Path.Combine(path, $@"problem{problemId}"));
        //     foreach (var sol in sols)
        //     {
        //         File.WriteAllText(Path.Combine(path, $"meta-{sol.ScoreEstimated}-{sol.SolverId}.txt"), sol.SolverMeta.ToJson());
        //         File.WriteAllText(Path.Combine(path, $"sol-{sol.ScoreEstimated}-{sol.SolverId}.txt"), sol.Solution);
        //     }
        // }
    }

    [Test]
    [Explicit]
    public void SaveBestPngs()
    {
        var path = FileHelper.FindDirectoryUpwards("best-solutions");
        var prIds = ScreenRepo.GetProblemIds();
        foreach(var problemId in prIds)
        {
            var solvers = SolutionRepo.GetAllSolvers(problemId).GetAwaiter().GetResult();
            foreach (var solver in solvers)
            {
                var sol = SolutionRepo.GetBestSolutionBySolverId(problemId, solver).GetAwaiter().GetResult();
                if (sol == null)
                {
                    Console.WriteLine("sol is null");
                    return;
                }
                Console.WriteLine($"{sol.SolverId} - {sol.ScoreEstimated}");
                var screen = ScreenRepo.GetProblem(problemId);
                var canvas = new Canvas(screen);
                var moves = Moves.Parse(sol.Solution);
                foreach (var move in moves)
                {
                    canvas.Apply(move);
                }

                var finalCanvas = canvas.ToScreen();
                finalCanvas.ToImage(Path.Combine(path, $"sol-{problemId}-{sol.SolverId}.png"));
            }
        }
    }

    [Test]
    [Explicit]
    public void GetAllBestStatByIds()
    {
        var stats = SolutionRepo.GetAllBestStats().GetAwaiter().GetResult();
        Console.WriteLine(string.Join("\n", stats.Select(e => $"{e.problem_id} {e.best_score} {e.solver_id}")));
        // var f = SolutionRepo.GetAllBestMetaStats().GetAwaiter().GetResult();
        // Console.WriteLine(string.Join("\n", f.Select(e => $"{e.problem_id} {e.best_score} {e.solver_meta}")));
    }
}
