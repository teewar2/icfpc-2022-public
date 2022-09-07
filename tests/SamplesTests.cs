using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using lib;
using lib.db;
using NUnit.Framework;

namespace tests;

public class SamplesTests
{
    [Test]
    public async Task Ydb_SampleInsert()
    {
        using var ydbRepo = new YdbMembersRepo(new Settings());
        await ydbRepo.UpsertMember(Environment.UserName, Environment.MachineName, DateTime.Now);
    }

    [Test]
    public async Task YandexObjectStorage_SamplePut()
    {
        var repo = new SubmissionRepo(new Settings());
        await repo.PutFile(Environment.UserName, Encoding.UTF8.GetBytes(Environment.MachineName));
        var content = await repo.GetFile(Environment.UserName);
        Console.WriteLine(Encoding.UTF8.GetString(content));
    }

    [Test]
    public void GoogleSheets_SampleReedWriteData()
    {
        var sheetClient = new GSheetClient();
        var sheet = sheetClient.GetSheetByUrl("https://docs.google.com/spreadsheets/d/1ukozztBYgtcPGJ4xrm-KGVp_gLNo1Bso5tXWV5Qqz5M/edit#gid=0");
        var data = sheet.ReadRange((0, 0), (1, 100));
        foreach (var r in data) Console.WriteLine(string.Join("\t", r));
        var row = data.FirstOrDefault(r => r[0] == Environment.UserName);
        if (row == null)
        {
            data.Add(new List<string> { Environment.UserName, Environment.MachineName, DateTime.Now.ToString(CultureInfo.InvariantCulture) });
        }
        else
        {
            row[1] = Environment.MachineName;
            row[2] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        sheet.Edit()
            .WriteRange((0, 0), data)
            .ColorizeRange((0, 0), (0, 2), new Color { Red = 1 })
            .Execute();
    }
}
