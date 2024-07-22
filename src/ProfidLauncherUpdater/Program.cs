//Config lesen

//Gibt es bereits eine Installation (-> First Time)

using Microsoft.Extensions.Hosting;
using ProfidLauncherUpdater.Commands;
using ProfidLauncherUpdater.Features;
using Spectre.Console;
using System.Reflection;

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

//if (args.Length == 0)
//{
AnsiConsole.Write(new FigletText("ProfidLauncher Updater").Centered().Color(Color.Blue));
//}

var version = "1.0.0+LOCALBUILD";
var appAssembly = typeof(Program).Assembly;
if (appAssembly != null)
{
    var attrs = appAssembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute));
    if (attrs != null)
    {
        var infoVerAttr = (AssemblyInformationalVersionAttribute)attrs;
        if (infoVerAttr != null && infoVerAttr.InformationalVersion.Length > 6)
        {
            version = infoVerAttr.InformationalVersion;
        }
    }
}

if (version.Contains('+'))
{
    version = version[..version.IndexOf('+')];
}

AnsiConsole.WriteLine();
AnsiConsole.WriteLine($"Version: {version}");
AnsiConsole.WriteLine();

return app.Run(args);