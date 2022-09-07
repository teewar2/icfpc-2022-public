using lib;
using lib.api;
using lib.db;
using Spectre.Console;

namespace submitter
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Contains("-handMode"))
            {
                Console.WriteLine("running handMode");
                var filePathSet = new HashSet<string>();
                var handsDirectory = FileHelper.FindDirectoryUpwards("hand-solutions");

                while (true)
                {
                    var filePaths = Directory.GetFiles(handsDirectory, "*.txt");
                    foreach (var filePath in filePaths)
                    {
                        if (filePathSet.Contains(filePath))
                            continue;
                        filePathSet.Add(filePath);
                        var fileName = Path.GetFileName(filePath);
                        var nameParts = fileName.Split('-');
                        if (!nameParts[0].Contains("problem"))
                            continue;
                        var problemId = int.Parse(nameParts[1]);
                        var program = File.ReadAllText(filePath);
                        var moves = Moves.Parse(program);
                        var screen = Screen.LoadProblem(problemId);
                        var score = screen.CalculateScore(moves);
                        var sol = SolutionRepo.GetBestSolutionBySolverId(problemId, "manual").GetAwaiter().GetResult();
                        if (sol != null)
                        {
                            if (sol.Solution == program)
                            {
                                Console.WriteLine($"duplicated solution {fileName}");
                                continue;
                            }

                            if (sol.ScoreEstimated <= score)
                            {
                                Console.WriteLine($"{fileName} with score {score} is not good enough - bestScore: {sol.ScoreEstimated}");
                                continue;
                            }
                        }

                        var solution = new ContestSolution(problemId, score, program, new SolverMeta(), "manual");
                        SolutionRepo.Submit(solution);
                        Console.WriteLine($"Send {fileName} to repo with score {score}");
                    }

                    Console.WriteLine("Sleeping");
                    Thread.Sleep(60_000);
                }
                // var scoresById = SolutionRepo.GetBestScoreByProblemId().GetAwaiter().GetResult();
                // foreach (var (problemId, score) in scoresById)
                // {
                //     var solution = SolutionRepo.GetSolutionByProblemIdAndScore(problemId, score).GetAwaiter().GetResult();
                //     Console.WriteLine(solution);
                // }
            }
            else
            {
                Console.WriteLine("running normalMode");
                var api = new Api();
                while (true)
                {
                    var submissionsInfos = api.GetSubmissionsInfo();
                    var problemIdToInfo = submissionsInfos!.Submissions.ToDictionary(s => s.Id, s => s);
                    var scoreByProblemId = SolutionRepo.GetBestScoreByProblemId().GetAwaiter().GetResult();
                    foreach (var (problemId, score) in scoreByProblemId)
                    {
                        Console.WriteLine($"Processing problem {problemId}");
                        var solution = SolutionRepo.GetSolutionByProblemIdAndScore(problemId, score).GetAwaiter().GetResult();
                        if (solution.SubmissionId == null)
                        {
                            var submissionResult = api.PostSolution(solution.ProblemId, solution.Solution);
                            solution.SubmittedAt = DateTime.UtcNow;
                            solution.SubmissionId = submissionResult?.Submission_Id;
                            Console.WriteLine($"Submit solution for problem {problemId} with score {score} by {solution.SolverId}");
                            SolutionRepo.Submit(solution);
                            continue;
                        }

                        if (solution.ScoreServer == null && problemIdToInfo.ContainsKey((long) solution.SubmissionId))
                        {
                            solution.ScoreServer = problemIdToInfo[solution.SubmissionId.Value].Score;
                            Console.WriteLine($"Add ScoreServer for problem {problemId} - score: {score}, serverScore: {solution.ScoreServer}, submissionId {solution.SubmissionId}");
                            SolutionRepo.Submit(solution);
                        }
                    }
                    Console.WriteLine("sleeping");
                    Thread.Sleep(300_000);
                }
            }
        }
    }
}
