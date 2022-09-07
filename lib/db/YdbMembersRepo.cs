using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Ydb.Sdk;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using Ydb.Sdk.Yc;

namespace lib.db;

public class YdbMembersRepo : IDisposable
{
    private readonly Settings settings;
    private Driver? driver;

    public YdbMembersRepo(Settings settings)
    {
        this.settings = settings;
    }

    public async Task UpsertMember(string username, string machineName, DateTime timestamp)
    {
        var client = await CreateTableClient();
        // YQL reference https://ydb.tech/ru/docs/yql/reference/
        // Примеры: https://github.com/ydb-platform/ydb-dotnet-examples/tree/main/src/BasicExample
        var response = await client.SessionExec(async session =>

            await session.ExecuteDataQuery(
                query: @"
                DECLARE $username AS Utf8;
                DECLARE $machine AS Utf8;
                DECLARE $timestamp AS Timestamp;
                UPSERT INTO members (username, machine, timestamp) VALUES ($username, $machine, $timestamp)",
                txControl: TxControl.BeginSerializableRW().Commit(),
                parameters: new Dictionary<string, YdbValue>
                {
                    { "$username", YdbValue.MakeUtf8(username)},
                    { "$machine", YdbValue.MakeUtf8(machineName)},
                    { "$timestamp", YdbValue.MakeTimestamp(timestamp)},
                }
        ));
        response.Status.EnsureSuccess();
    }

    private async Task<TableClient> CreateTableClient()
    {
        var config = new DriverConfig(
            endpoint: settings.YdbEndpoint,
            database: settings.YdbDatabase,
            credentials: new ServiceAccountProvider(settings.YandexCloudKeyFile),
            customServerCertificate: YcCerts.GetDefaultServerCertificate()
        );

        driver = new Driver(
            config: config,
            loggerFactory: new NullLoggerFactory()
        );

        await driver.Initialize();

        return new TableClient(driver, new TableClientConfig());
    }

    public void Dispose()
    {
        if (driver != null)
            driver.Dispose();
    }
}
