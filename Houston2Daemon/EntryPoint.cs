using Houston2Daemon;
using Vostok.Hosting;
using Vostok.Hosting.Houston;
using Vostok.Hosting.Houston.Abstractions;
using Vostok.Hosting.Houston.Configuration;
using Vostok.Hosting.Setup;

[assembly: HoustonEntryPoint(typeof(YDBRatingVisualizerApplication))]

await new HoustonHost(new YDBRatingVisualizerApplication(), ConfigureHost)
    .WithConsoleCancellation()
    .RunAsync();

static void ConfigureHost(IHostingConfiguration config)
{
    config.OutOfHouston.SetupEnvironment(EnvironmentSetup);
}

static void EnvironmentSetup(IVostokHostingEnvironmentBuilder builder)
{
    builder
        .SetupApplicationIdentity(identityBuilder => identityBuilder
            .SetProject("YDBRatingVisualizer")
            .SetApplication("ICFPC2022")
            .SetEnvironment("Cloud"))
        .SetupLog(logBuilder => logBuilder
            .SetupConsoleLog())
        .SetupConfiguration(config =>
        {
            config.CustomizeConfigurationContext(c =>
                c.ConfigurationProvider.SetupSourceFor<Secrets>(c.ConfigurationSource));
        });
}
