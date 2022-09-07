using lib;
using lib.db;
using lib.Enhancers;

namespace enhancer
{
    internal static class Program
    {
        // private static SolutionRepo2 SolutionRepo = new SolutionRepo2();
        private static void Main(string[] args)
        {
            var excludedAlgoPrefixes = new HashSet<string> { "Layers", "Simple" };

            while (true)
            {
                var scoreByProblemId = SolutionRepo.GetBestScoreByProblemIdAndSolverId(excludedAlgoPrefixes.ToList()).GetAwaiter().GetResult();

                Console.WriteLine($"getting {scoreByProblemId.Count} solutions from DB");
                var solutions = scoreByProblemId.ToDictionary(
                    e => (e.problemId, e.solverId),
                    e => SolutionRepo.GetSolutionByProblemIdAndSolverIdAndScore(e.problemId, e.solverId, e.score).GetAwaiter().GetResult());
                Console.WriteLine("got solutions from DB");

                Parallel.ForEach(scoreByProblemId, parms =>
                {
                    var (problemId, solverId, score) = parms;
                    var solution = solutions[(problemId, solverId)];
                    solution.SolverMeta.Enhanced_By ??= new List<string>();
                    var screen = ScreenRepo.GetProblem((int)solution.ProblemId);

                    // enhancer 1
                    if (!solution.SolverMeta.Enhanced_By.Contains("enchancer"))
                    {
                        Console.WriteLine($"solution {solution.SolverId} for problem {solution.ProblemId} not enhanced");
                        var eMoves = Enhancer.Enhance(screen, Moves.Parse(solution.Solution));
                        var eSolution = new ContestSolution(
                            solution.ProblemId,
                            screen.CalculateScore(eMoves),
                            eMoves.StrJoin("\n"),
                            new SolverMeta(solution.ScoreEstimated, solution.SolverId),
                            solution.SolverId + "-enchanced");
                        Console.WriteLine($"solution {solution.SolverId} for problem {solution.ProblemId} enhanced from score {solution.ScoreEstimated} to {eSolution.ScoreEstimated}");
                        SolutionRepo.Submit(eSolution);

                        solution.SolverMeta.Enhanced_By.Add("enchancer");
                        SolutionRepo.Submit(solution);
                    } else Console.WriteLine($"skipping enchancer for solution {solution.SolverId} for problem {solution.ProblemId} with meta {solution.SolverMeta}");

                    // enhancer 2
                    if (!solution.SolverMeta.Enhanced_By.Contains("enchancer2"))
                    {
                        Console.WriteLine($"solution {solution.SolverId} for problem {solution.ProblemId} not enhanced2");
                        var eMoves = Enhancer.Enhance2(screen, Moves.Parse(solution.Solution));
                        var eSolution = new ContestSolution(
                            solution.ProblemId,
                            screen.CalculateScore(eMoves),
                            eMoves.StrJoin("\n"),
                            new SolverMeta(solution.ScoreEstimated, solution.SolverId),
                            solution.SolverId + "-2-enchanced");
                        Console.WriteLine($"solution {solution.SolverId} for problem {solution.ProblemId} enhanced2 from score {solution.ScoreEstimated} to {eSolution.ScoreEstimated}");
                        SolutionRepo.Submit(eSolution);

                        solution.SolverMeta.Enhanced_By.Add("enchancer2");
                        SolutionRepo.Submit(solution);
                    } else Console.WriteLine($"skipping enchancer2 for solution {solution.SolverId} for problem {solution.ProblemId} with meta {solution.SolverMeta}");
                });

                Console.WriteLine("sleeping");
                Thread.Sleep(60_000);
            }
        }
    }
}
