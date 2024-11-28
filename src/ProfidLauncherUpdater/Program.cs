using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProfidLauncherUpdater.Commands;
using ProfidLauncherUpdater.Features;
using ProfidLauncherUpdater.Infrastructure.SelfUpdate;
using Serilog;
using Spectre.Console;

var host = Host.CreateApplicationBuilder();

host.Logging.ClearProviders();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(host.Configuration)
    .CreateLogger();

host.Logging.AddSerilog();

host.Services.AddFeatures(host.Configuration);

var app = new CommandApp(new TypeRegistrar(host.Services));

app.Configure(c =>
{
    c.AddCommand<LaunchCommand>("run")
    .WithExample(["run ATRIUMP"])
    .WithDescription("Launches the profid launcher with the given mode");

    c.AddCommand<CheckInstallationCommand>("check")
    .WithExample(["check"])
    .WithDescription("Checks the current state of the profid launcher installation");

    c.AddCommand<UpdateCommand>("update")
    .WithExample(["update"])
    .WithDescription("Checks for a new update and updates the app if there is a newer one. It also does a first time installation!");
});

AnsiConsole.Write(new FigletText("ProfidLauncher Updater").Centered().Color(Color.Blue));

var version = Tools.GetCurrentVersion();

Log.Logger.Information($"ProfidLauncher Updater v{version}");

AnsiConsole.WriteLine();
AnsiConsole.WriteLine($"Version: {version}");
AnsiConsole.WriteLine();

var selfUpdater = new SelfUpdater(host.Configuration, Log.Logger);

var sVersion = await selfUpdater.GetServerVersion();
Log.Logger.Information($"Server version v{sVersion.Value!.version}");

if (sVersion.IsFailure)
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine($"[red]{sVersion.Error!.Description}[/]");
    AnsiConsole.WriteLine();
}

if (sVersion.IsSuccess)
{
    AnsiConsole.WriteLine();
    AnsiConsole.WriteLine($"Server Version: {sVersion.Value!.version}");
    AnsiConsole.WriteLine();

#if DEBUG
    //version = sVersion.Value!.version;
#endif

    if (sVersion.Value!.version != version)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[blue]Update of updator required![/]");
        AnsiConsole.WriteLine();

        if (!args.Contains("check"))
        {
            Log.Logger.Information($"Requires self update...");
            var updaterResult = await selfUpdater.UpdateSelf(sVersion.Value!.serverVersion);
            updaterResult.Switch(
                success: (v) =>
                {
                    Log.Logger.Information($"Update run, need to close application");
                    Environment.Exit(0);
                },
                failure: (err) =>
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[red]{err.Description}[/]");
                    AnsiConsole.WriteLine();
                },
                notFound: (err) =>
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[red]{err.Description}[/]");
                    AnsiConsole.WriteLine();
                }
                );
        }
    }
}

return app.Run(args);