using Microsoft.Extensions.Hosting;
using ProfidLauncherUpdater.Commands;
using ProfidLauncherUpdater.Features;
using ProfidLauncherUpdater.Infrastructure.SelfUpdate;
using Spectre.Console;

var host = Host.CreateApplicationBuilder();

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

//if (args.Length == 0)
//{
AnsiConsole.Write(new FigletText("ProfidLauncher Updater").Centered().Color(Color.Blue));
//}

var version = Tools.GetCurrentVersion();

AnsiConsole.WriteLine();
AnsiConsole.WriteLine($"Version: {version}");
AnsiConsole.WriteLine();

var selfUpdater = new SelfUpdater(host.Configuration);

var sVersion = await selfUpdater.GetServerVersion();
if (sVersion.IsFailure)
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine($"[red]{sVersion.Error!.Description}[/]");
    AnsiConsole.WriteLine();
}

if (sVersion.IsSuccess)
{
    AnsiConsole.WriteLine();
    AnsiConsole.WriteLine($"Server Version: {sVersion.Value!.Version}");
    AnsiConsole.WriteLine();

    if (sVersion.Value!.Version != version)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[blue]Update of updator required![/]");
        AnsiConsole.WriteLine();

        var updaterResult = await selfUpdater.UpdateSelf(sVersion.Value!);
        updaterResult.Switch(
            success: (v) => Environment.Exit(0),
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

return app.Run(args);