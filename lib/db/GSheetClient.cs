using System.IO;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace lib.db
{
    public class GSheetClient
    {
        public static Regex UrlRegex = new Regex("https://docs.google.com/spreadsheets/d/(.+)/edit#gid=(.+)", RegexOptions.Compiled);

        public GSheetClient()
        {
            SheetsService = new SheetsService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = GoogleCredential.FromJson(new Settings().GoogleAuthJson)
                        .CreateScoped(SheetsService.Scope.Spreadsheets),
                    ApplicationName = "icfpc21-pegovka-client"
                });
        }

        public GSpreadsheet GetSpreadsheet(string spreadsheetId) =>
            new(spreadsheetId, SheetsService);

        public GSheet GetSheetByUrl(string url)
        {
            var match = UrlRegex.Match(url);
            var spreadsheetId = match.Groups[1].Value;
            var sheetId = int.Parse(match.Groups[2].Value);
            return GetSpreadsheet(spreadsheetId).GetSheetById(sheetId);
        }

        private SheetsService SheetsService { get; }
    }
}
