//Config lesen

//Gibt es bereits eine Installation (-> First Time)

using Microsoft.Extensions.Hosting;
using ProfidLauncherUpdater.Commands;
using ProfidLauncherUpdater.Features;
using Spectre.Console;

var host = Host.CreateApplicationBuilder();

host.Services.AddFeatures(host.Configuration);
host.Services.AddScoped<IAnsiConsoleService, AnsiConsoleService>();

var app = new CommandApp(new TypeRegistrar(host));

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

if (args.Length == 0)
{
    AnsiConsole.Write(new FigletText("ProfidLauncher Updater").Centered().Color(Color.Blue));
}

return app.Run(args);