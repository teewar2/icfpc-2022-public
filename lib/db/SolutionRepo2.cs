using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Ydb.Sdk;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using Ydb.Sdk.Yc;

namespace lib.db;

// public class SolutionRepo2
// {
//     private readonly TableClient client;
//
//     public async Task SubmitAsync(ContestSolution solution)
//     {
//         // Console.WriteLine("SubmitAsync");
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: @"
//                 DECLARE $id AS Utf8;
//                 DECLARE $problem_id AS Int64;
//                 DECLARE $score_estimated AS Int64;
//                 DECLARE $score_server AS Int64?;
//                 DECLARE $solution AS Utf8;
//                 DECLARE $solved_at AS Datetime;
//                 DECLARE $solver_id AS Utf8;
//                 DECLARE $solver_meta AS Json;
//                 DECLARE $submission_id AS Int64?;
//                 DECLARE $submitted_at AS Datetime?;
//                 UPSERT INTO Solutions (id, problem_id, score_estimated, score_server,solution, solved_at, solver_id, solver_meta, submission_id, submitted_at) VALUES ($id, $problem_id, $score_estimated, $score_server, $solution, $solved_at, $solver_id, $solver_meta, $submission_id, $submitted_at)",
//                 txControl: TxControl.BeginStaleRO().Commit(),
//                 parameters: new Dictionary<string, YdbValue>
//                 {
//                     { "$id", YdbValue.MakeUtf8(solution.Id.ToString())},
//                     { "$problem_id", YdbValue.MakeInt64(solution.ProblemId)},
//                     { "$score_estimated", YdbValue.MakeInt64(solution.ScoreEstimated)},
//                     { "$score_server", solution.ScoreServer == null ? YdbValue.MakeEmptyOptional(YdbTypeId.Int64) : YdbValue.MakeOptional(YdbValue.MakeInt64((long) solution.ScoreServer))},
//                     { "$solution", YdbValue.MakeUtf8(solution.Solution)},
//                     { "$solved_at", YdbValue.MakeDatetime(solution.SolvedAt)},
//                     { "$solver_id", YdbValue.MakeUtf8(solution.SolverId)},
//                     { "$solver_meta", YdbValue.MakeJson(solution.SolverMeta.ToJson())},
//                     { "$submission_id", solution.SubmissionId == null ? YdbValue.MakeEmptyOptional(YdbTypeId.Int64) : YdbValue.MakeOptional(YdbValue.MakeInt64((long) solution.SubmissionId))},
//                     { "$submitted_at", solution.SubmittedAt == null ? YdbValue.MakeEmptyOptional(YdbTypeId.Datetime) : YdbValue.MakeOptional(YdbValue.MakeDatetime((DateTime) solution.SubmittedAt))},
//                 }
//             ));
//         response.Status.EnsureSuccess();
//     }
//
//     public  void Submit(ContestSolution solution)
//     {
//         SubmitAsync(solution).GetAwaiter().GetResult();
//     }
//
//     public  async Task<List<(long problemId, long score)>> GetBestScoreByProblemId()
//     {
//         // Console.WriteLine("GetBestScoreByProblemId");
//         var ans = new List<(long, long)>();
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: @"SELECT problem_id, min(score_estimated) AS score FROM Solutions GROUP BY problem_id;",
//                 txControl: TxControl.BeginStaleRO().Commit(),
//                 parameters: new Dictionary<string, YdbValue> {}
//
//             ));
//         response.Status.EnsureSuccess();
//         var queryResponse = (ExecuteDataQueryResponse)response;
//         foreach (var row in queryResponse.Result.ResultSets[0].Rows)
//         {
//             var problemId = (long?) row["problem_id"] ?? throw new ArgumentException();
//             var scoreEstimated = (long?) row["score"] ?? throw new ArgumentException();
//             ans.Add(new (problemId, scoreEstimated));
//         }
//
//         return ans;
//     }
//
//     public  async Task<List<(long problemId, string solverId, long score)>> GetBestScoreByProblemIdAndSolverId(List<string> ignoreSolverPrefixes)
//     {
//         // Console.WriteLine("GetBestScoreByProblemIdAndSolverId");
//         var ans = new List<(long, string, long)>();
//         var ignoreClause = ignoreSolverPrefixes.StrJoin("|");
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: $"SELECT problem_id, solver_id, min(score_estimated) AS score FROM Solutions WHERE solver_id NOT REGEXP '^({ignoreClause})' AND solver_id NOT REGEXP '-enchanced$' GROUP BY problem_id, solver_id",
//                 txControl: TxControl.BeginStaleRO().Commit(),
//                 parameters: new Dictionary<string, YdbValue> {}
//
//             ));
//         response.Status.EnsureSuccess();
//         var queryResponse = (ExecuteDataQueryResponse)response;
//         foreach (var row in queryResponse.Result.ResultSets[0].Rows)
//         {
//             var problemId = (long?) row["problem_id"] ?? throw new ArgumentException();
//             var solverId = (string?)row["solver_id"] ?? throw new ArgumentException();
//             var scoreEstimated = (long?) row["score"] ?? throw new ArgumentException();
//             ans.Add(new (problemId, solverId, scoreEstimated));
//         }
//
//         return ans;
//     }
//
//     public  async Task<ContestSolution> GetSolutionByProblemIdAndScore(long problemId, long scoreEstimated)
//     {
//         // Console.WriteLine("GetSolutionByProblemIdAndScore");
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: @"
//                 DECLARE $problem_id AS Int64;
//                 DECLARE $score_estimated AS Int64;
//
//                 SELECT * FROM Solutions WHERE problem_id=$problem_id AND score_estimated=$score_estimated LIMIT 1",
//                 txControl: TxControl.BeginStaleRO().Commit(),
//                 parameters: new Dictionary<string, YdbValue>
//                 {
//                     { "$problem_id", YdbValue.MakeInt64(problemId)},
//                     { "$score_estimated", YdbValue.MakeInt64(scoreEstimated)},
//                 }
//
//             ));
//         response.Status.EnsureSuccess();
//         var queryResponse = (ExecuteDataQueryResponse) response;
//         var row = queryResponse.Result.ResultSets[0].Rows.First();
//
//         var ans = new ContestSolution(row);
//         return ans;
//     }
//
//     public  async Task<ContestSolution> GetSolutionByProblemIdAndSolverIdAndScore(long problemId, string solverId, long scoreEstimated)
//     {
//         Console.Write("GetSolutionByProblemIdAndSolverIdAndScore ");
//         var sw = Stopwatch.StartNew();
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: @"
//                 DECLARE $problem_id AS Int64;
//                 DECLARE $solver_id AS Utf8;
//                 DECLARE $score_estimated AS Int64;
//
//                 SELECT * FROM Solutions WHERE problem_id=$problem_id AND solver_id=$solver_id AND score_estimated=$score_estimated ORDER BY solved_at DESC LIMIT 1",
//                 txControl: TxControl.BeginStaleRO().Commit(),
//                 parameters: new Dictionary<string, YdbValue>
//                 {
//                     { "$problem_id", YdbValue.MakeInt64(problemId)},
//                     { "$solver_id", YdbValue.MakeUtf8(solverId)},
//                     { "$score_estimated", YdbValue.MakeInt64(scoreEstimated)},
//                 }
//
//             ));
//         response.Status.EnsureSuccess();
//         // Console.WriteLine(sw.Elapsed);
//         var queryResponse = (ExecuteDataQueryResponse) response;
//         var row = queryResponse.Result.ResultSets[0].Rows.First();
//
//         var ans = new ContestSolution(row);
//         return ans;
//     }
//
//     public  async Task<ContestSolution?> GetBestSolutionBySolverId(long problemId, string solverId)
//     {
//         // Console.WriteLine($"GetBestSolutionBySolverId {problemId} {solverId}");
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: @"
//                 DECLARE $problem_id AS Int64;
//                 DECLARE $solver_id AS Utf8;
//
//                 SELECT * FROM Solutions WHERE problem_id=$problem_id AND solver_id=$solver_id ORDER BY score_estimated LIMIT 1",
//                 txControl: TxControl.BeginStaleRO().Commit(),
//                 parameters: new Dictionary<string, YdbValue>
//                 {
//                     { "$problem_id", YdbValue.MakeInt64(problemId)},
//                     { "$solver_id", YdbValue.MakeUtf8(solverId)},
//                 }
//
//             ));
//         response.Status.EnsureSuccess();
//         var queryResponse = (ExecuteDataQueryResponse) response;
//         var row = queryResponse.Result.ResultSets[0].Rows.FirstOrDefault();
//
//         if (row == null)
//             return null;
//         var ans = new ContestSolution(row);
//         return ans;
//     }
//
//     public  async Task<List<ContestSolution>> GetAllSolutions()
//     {
//         // Console.WriteLine("GetAllSolutions");
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: @"SELECT * FROM Solutions",
//                 txControl: TxControl.BeginStaleRO().Commit()
//             ));
//         response.Status.EnsureSuccess();
//         // Console.WriteLine("Done");
//         var queryResponse = (ExecuteDataQueryResponse) response;
//         return queryResponse.Result.ResultSets[0].Rows.Select(r => new ContestSolution(r)).ToList();
//     }
//
//     public  async Task<ContestSolution?> GetBestSolutionByProblemId(long problemId)
//     {
//         // Console.WriteLine("GetBestSolutionByProblemId");
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: @"
//                 DECLARE $problem_id AS Int64;
//
//                 SELECT * FROM Solutions WHERE problem_id=$problem_id ORDER BY score_estimated LIMIT 1",
//                 txControl: TxControl.BeginStaleRO().Commit(),
//                 parameters: new Dictionary<string, YdbValue>
//                 {
//                     { "$problem_id", YdbValue.MakeInt64(problemId)},
//                 }
//
//             ));
//         response.Status.EnsureSuccess();
//         var queryResponse = (ExecuteDataQueryResponse) response;
//         var row = queryResponse.Result.ResultSets[0].Rows.FirstOrDefault();
//         if (row == null)
//             return null;
//         var ans = new ContestSolution(row);
//         return ans;
//     }
//
//     public  async Task<string[]> GetAllSolvers(long problemId)
//     {
//         // Console.WriteLine("GetAllSolvers");
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: @"
//                 DECLARE $problem_id AS Int64;
//
//                 SELECT DISTINCT solver_id FROM Solutions WHERE problem_id=$problem_id",
//                 txControl: TxControl.BeginStaleRO().Commit(),
//                 parameters: new Dictionary<string, YdbValue>
//                 {
//                     { "$problem_id", YdbValue.MakeInt64(problemId)},
//                 }
//
//             ));
//         response.Status.EnsureSuccess();
//         var queryResponse = (ExecuteDataQueryResponse) response;
//         return queryResponse.Result.ResultSets[0].Rows.Select(x => (string?)x["solver_id"] ?? throw new Exception("WTF")).ToArray();
//     }
//
//     public  async Task<long[]> GetAllSolutionIds()
//     {
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: @"SELECT * FROM Solutions",
//                 txControl: TxControl.BeginStaleRO().Commit(),
//                 parameters: new Dictionary<string, YdbValue>()));
//         response.Status.EnsureSuccess();
//         var queryResponse = (ExecuteDataQueryResponse) response;
//         return queryResponse.Result.ResultSets[0].Rows.Select(x => (long?)x["problem_id"] ?? throw new Exception("WTF")).ToArray();
//     }
//
//     public  async Task<long[]> GetAllProblems()
//     {
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: @"SELECT DISTINCT problem_id FROM Solutions",
//                 txControl: TxControl.BeginStaleRO().Commit(),
//                 parameters: new Dictionary<string, YdbValue>()));
//         response.Status.EnsureSuccess();
//         var queryResponse = (ExecuteDataQueryResponse) response;
//         return queryResponse.Result.ResultSets[0].Rows.Select(x => (long?)x["problem_id"] ?? throw new Exception("WTF")).ToArray();
//     }
//     public record ProblemStat(long problem_id, long? best_score, string? solver_id);
//
//     public  async Task<List<ProblemStat>> GetAllBestStats()
//     {
//         var response = await client.SessionExec(async session =>
//
//             await session.ExecuteDataQuery(
//                 query: @$"
//                 select * from (select problem_id, min(score_estimated) as best_score, solver_id from Solutions group by problem_id, solver_id);",
//                 txControl: TxControl.BeginStaleRO().Commit(),
//                 parameters: new Dictionary<string, YdbValue>
//                 {
//                 }
//             ));
//         response.Status.EnsureSuccess();
//         var ans = new List<ProblemStat>();
//         var queryResponse = (ExecuteDataQueryResponse) response;
//         foreach (var row in queryResponse.Result.ResultSets[0].Rows)
//         {
//             var problemId = (long?) row["problem_id"] ?? throw new ArgumentException();
//             var solverId = (string?)row["solver_id"] ?? throw new ArgumentException();
//             var bestScore = (long?) row["best_score"] ?? throw new ArgumentException();
//             ans.Add(new ProblemStat(problemId, bestScore, solverId));
//         }
//         return ans;
//     }
//
//     private  async Task<TableClient> CreateTableClient()
//     {
//         // Console.WriteLine("CreateTableClient");
//         var settings = new Settings();
//         var config = new DriverConfig(
//             endpoint: settings.YdbEndpoint,
//             database: settings.YdbDatabase,
//             credentials: new ServiceAccountProvider(FileHelper.FindFilenameUpwards(settings.YandexCloudKeyFile)),
//             customServerCertificate: YcCerts.GetDefaultServerCertificate()
//         );
//
//         var driver = new Driver(
//             config: config,
//             loggerFactory: new NullLoggerFactory()
//         );
//
//         await driver.Initialize();
//
//         return new TableClient(driver, new TableClientConfig());
//     }
//
//     public SolutionRepo2()
//     {
//         client = CreateTableClient().GetAwaiter().GetResult();
//     }
// }
