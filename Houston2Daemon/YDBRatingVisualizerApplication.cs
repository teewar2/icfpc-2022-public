using System.Globalization;
using Google.Apis.Sheets.v4.Data;
using lib;
using lib.api;
using lib.db;
using Vostok.Applications.Scheduled;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Logging.Abstractions;

namespace Houston2Daemon;

[RequiresSecretConfiguration(typeof(Secrets))]
public class YDBRatingVisualizerApplication : VostokScheduledApplication
{
    private const int SolverTypes = 3;
    private const string E = "-enchanced";
    private const string E2 = $"-2{E}";
    private List<string> headers = new() { "ProblemId", "Type", "Top1 score", "Top1 solver", "Top2 score", "Top2 solver", "Top3 score", "Top3 solver" };
    private GSheetClient gsClient = new();
    private Secrets? secrets;

    public override void Setup(IScheduledActionsBuilder builder, IVostokHostingEnvironment environment)
    {
        secrets = environment.SecretConfigurationProvider.Get<Secrets>();

        builder.Schedule(
            "Update",
            Scheduler.Periodical(() => 1.Minutes()),
            () => PerformIteration(environment));
    }

    private void PerformIteration(IVostokHostingEnvironment environment)
    {
        environment.Log.Info("Start updating..");
        var bestStats = SolutionRepo.GetAllBestStats().GetAwaiter().GetResult();
        var bestMetaStats = SolutionRepo.GetAllBestMetaStats().GetAwaiter().GetResult();
        environment.Log.Info("Start executing stats builder..");
        BuildStats(bestStats);
        environment.Log.Info("Start executing better stats builder..");
        BuildBestStat(bestStats, bestMetaStats);
        environment.Log.Info("Finished updating");
    }

    private void BuildStats(List<SolutionRepo.ProblemStat> bestStats)
    {
        var spreadSheet = gsClient.GetSpreadsheet(secrets!.SpreadSheetId!);
        var sheet = spreadSheet.GetSheetByName(secrets.SpreadSheetName!);
        var data = new List<List<string>> {headers};
        var baseDict = new Dictionary<long, List<(long?, string?)>>();
        var e1Dict = new Dictionary<long, List<(long?, string?)>>();
        var e2Dict = new Dictionary<long, List<(long?, string?)>>();

        foreach (var (prId, score, solverId) in bestStats)
        {
            if (solverId is null)
                continue;

            if (solverId.EndsWith(E2))
            {
                var el = (score, id: solverId.Replace(E2, ""));
                e2Dict.AddOrUpdate(prId, new List<(long?, string?)> {el}, upd => upd.Add(el));
            }
            else if (solverId.EndsWith(E))
            {
                var el = (score, id: solverId.Replace(E, ""));
                e1Dict.AddOrUpdate(prId, new List<(long?, string?)> {el}, upd => upd.Add(el));
            }
            else
            {
                var el = (score, id: solverId);
                baseDict.AddOrUpdate(prId, new List<(long?, string?)> {el}, upd => upd.Add(el));
            }
        }

        foreach (var prId in baseDict.Keys)
        {
            data.Add(FormatLine(prId, new List<string> {prId.ToString()}, baseDict, "Base"));
            data.Add(FormatLine(prId, new List<string> {" "}, e1Dict, "Ench1"));
            data.Add(FormatLine(prId, new List<string> {" "}, e2Dict, "Ench2"));
        }

        var builder = sheet.Edit();
        builder.ClearAll().WriteRange((0, 0), data);
        foreach (var pr in baseDict.Keys)
        {
            var rowNumber = (int) pr * SolverTypes;
            var rangeStart = (rowNumber, 0);
            var rangeEnd = (rowNumber, headers.Count - 1);
            builder.AddBottomBorders(rangeStart, rangeEnd, new Color());
        }


        builder.Execute();
    }

    public void BuildBestStat(List<SolutionRepo.ProblemStat> bestStats, List<SolutionRepo.ProblemStatWithMeta> problemStatWithMetas)
    {
        var spreadSheet = gsClient.GetSpreadsheet(secrets!.SpreadSheetId!);
        var sheet = spreadSheet.GetSheetByName(secrets.BetterSpreadSheetName!);

        var baseDict = new Dictionary<long, List<(long?, string?, string?)>>();
        var e1Dict = new Dictionary<long, List<(long?, string?, string?)>>();
        var e2Dict = new Dictionary<long, List<(long?, string?, string?)>>();

        var api = new Api();
        var serverResults = api.GetResults();
        var metasDict = problemStatWithMetas.ToDictionary(s => s.problem_id, s => s.solver_meta);

        foreach (var (prId, score, solverId) in bestStats)
        {
            if (solverId is null)
                continue;

            if (solverId.EndsWith(E2))
            {
                var el = (score, id: solverId.Replace(E2, ""), metasDict.ContainsKey(prId) ? metasDict[prId] : " ");
                e2Dict.AddOrUpdate(prId, new List<(long?, string?, string?)> {el}, upd => upd.Add(el));
            }
            else if (solverId.EndsWith(E))
            {
                var el = (score, id: solverId.Replace(E, ""), metasDict.ContainsKey(prId) ? metasDict[prId] : " ");
                e1Dict.AddOrUpdate(prId, new List<(long?, string?, string?)> {el}, upd => upd.Add(el));
            }
            else
            {
                var el = (score, id: solverId, metasDict.ContainsKey(prId) ? metasDict[prId] : " ");
                baseDict.AddOrUpdate(prId, new List<(long?, string?, string?)> {el}, upd => upd.Add(el));
            }
        }

        var result = new Dictionary<string, int>();

        for (int i = 1; i <= baseDict.Keys.Count; i++)
            foreach (var dict in new[] {baseDict, e1Dict, e2Dict})
            foreach (var bestSolver in GetBestSolvers(i, dict))
            {
                result.AddOrUpdate(bestSolver, 1, upd => upd + 1);
            }

        var f = result.OrderByDescending(e => e.Value).Select(e => e.Key);

        var maxProblemId = baseDict.Keys.Max();
        var headers = new[]
        {
            DateTime.UtcNow.AddHours(5).ToString("t", CultureInfo.InvariantCulture),
            "Type"
        }.Concat(Enumerable.Range(1, (int) maxProblemId).Select(i => "#"+i.ToString())).Concat(new[] {DateTime.UtcNow.AddHours(5).ToString("t", CultureInfo.InvariantCulture)}).ToList();

        var bestResults = new[]
        {
            "Overall",
            " "
        }.Concat(serverResults is null ? new List<string>() : serverResults.results.OrderBy(i => i.problem_id).Select(e=>e.overall_best_cost.ToString())).Concat(new[] {"Overall"}).ToList();

        var bestOurResult = new[]
        {
            "OurBest",
            " "
        }.Concat(serverResults is null ? new List<string>() : serverResults.results.OrderBy(i => i.problem_id).Select(e=>e.min_cost.ToString())).Concat(new[] {"OurBest"}).ToList();

        var diff = new[]
        {
            "Diff",
            " "
        }.Concat(serverResults is null ? new List<string>() : serverResults.results.OrderBy(i => i.problem_id).Select(e=>(e.min_cost -e.overall_best_cost).ToString())).Concat(new[] {"Diff"}).ToList();

        var data = new List<List<string>> {headers, diff, new(), new(), bestResults, bestOurResult, new()};

        foreach (var solverId in f)
        {
            data.Add(FormatLine(solverId, new List<string> {solverId, "Base"}, baseDict, maxProblemId).Append("Base").Append(solverId).ToList());
            data.Add(FormatLine(solverId, new List<string> {" ", "Ench1"}, e1Dict, maxProblemId).Append("Ench1").ToList());
            data.Add(FormatLine(solverId, new List<string> {" ", "Ench2"}, e2Dict, maxProblemId).Append("Ench2").ToList());
        }

        data.Add(GetBestDescription());

        static List<string> FormatLine(string solverId, List<string> result, Dictionary<long, List<(long?, string?, string?)>> dict, long maxProblemId)
        {
            for (int problemId = 1; problemId <= maxProblemId; problemId++)
            {
                var value = dict.ContainsKey(problemId) && dict[problemId].Any(e => e.Item2 == solverId)
                    ? dict[problemId].Where(e => e.Item2 == solverId).MinBy(e => e.Item1).Item1.ToString()
                    : " ";
                result.Add(value!);
            }

            return result;
        }

        List<string> GetBestDescription()
        {
            var result = new List<string> { " ", " " };

            for (var problemId = 0; problemId < maxProblemId; problemId++)
            {
                var minScore = double.PositiveInfinity;
                string? minDescription = null!;

                if (baseDict.ContainsKey(problemId) && baseDict[problemId].Any())
                {
                    var (baseScore, _, baseDescription) = baseDict[problemId].MinBy(e => e.Item1);
                    if (baseScore < minScore)
                    {
                        minScore = baseScore.Value;
                        minDescription = baseDescription;
                    }
                }

                if (e1Dict.ContainsKey(problemId) && baseDict[problemId].Any())
                {
                    var (e1Score, _, e1Description) = e1Dict[problemId].MinBy(e => e.Item1);
                    if (e1Score < minScore)
                    {
                        minScore = e1Score.Value;
                        minDescription = e1Description;
                    }
                }

                if (e2Dict.ContainsKey(problemId) && baseDict[problemId].Any())
                {
                    var (e2Score, _, e2Description) = e2Dict[problemId].MinBy(e => e.Item1);
                    if (e2Score < minScore)
                    {
                        minScore = e2Score.Value;
                        minDescription = e2Description;
                    }
                }
                result.Add(minDescription ?? " ");
            }

            return result;
        }

        var builder = sheet.Edit();
        builder.ClearAll().WriteRange((0, 0), data);
        builder.Execute();
    }

    public static IEnumerable<string> GetBestSolvers(long prId, Dictionary<long, List<(long?, string?, string?)>> dict)
    {
        return dict.ContainsKey(prId)
            ? dict[prId]
                .Where(e => e.Item1.HasValue && e.Item2 != null)
                .OrderBy(e => e.Item1)
                .Take(1)
                .Select(e => e.Item2!)
            : Enumerable.Empty<string>();
    }

    public static List<string> FormatLine(long prId, List<string> result, Dictionary<long, List<(long?, string?, string?)>> dict, string solverType)
    {
        var top3 = dict.ContainsKey(prId)
            ? dict[prId]
                .Where(e => e.Item1.HasValue && e.Item2 != null)
                .OrderBy(e => e.Item1)
                .Take(3)
                .ToArray()
            : Array.Empty<(long?, string?, string?)>();

        result.Add(solverType);
        foreach (var (score, solver, _) in top3)
        {
            result.Add(score!.Value.ToString());
            result.Add(solver!);
        }
        return result;
    }

    public static List<string> FormatLine(long prId, List<string> result, Dictionary<long, List<(long?, string?)>> dict, string solverType)
    {
        var top3 = dict.ContainsKey(prId)
            ? dict[prId]
                .Where(e => e.Item1.HasValue && e.Item2 != null)
                .OrderBy(e => e.Item1)
                .Take(3)
                .ToArray()
            : Array.Empty<(long?, string?)>();

        result.Add(solverType);
        foreach (var (score, solver) in top3)
        {
            result.Add(score!.Value.ToString());
            result.Add(solver!);
        }
        return result;
    }
}
