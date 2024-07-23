using MediatR;
using ProfidLauncherUpdater.Features.Installation;
using ProfidLauncherUpdater.Features.Launch;
using Spectre.Console;
using System.ComponentModel;

namespace ProfidLauncherUpdater.Commands;

public class LaunchCommand : AsyncCommand<LaunchCommand.Settings>
{
    private readonly IMediator _mediator;

    public class Settings : CommandSettings
    {
        [Description("Mode to launch the profid launcher")]
        [CommandArgument(0, "<Mode>")]
        public string Mode { get; set; } = "";
    }

    public LaunchCommand(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine($"Launching ProfidLauncher in {settings.Mode} mode...");
        var result = await _mediator.Send(new LaunchApplication.Command(settings.Mode));
        if (result.IsFailure)
        {
            AnsiConsole.MarkupLine($"[red]{result.Error!.Description}[/]");
            return -1;
        }

        //Nun auf updates prüfen
        AnsiConsole.MarkupLine($"Checking for updates...");
        var uResult = await _mediator.Send(new Update.Command());
        if (uResult.IsFailure)
        {
            AnsiConsole.MarkupLine($"[red]{uResult.Error!.Description}[/]");
            return -1;
        }

        if (!result.Value)
        {
            //App war im ersten Versuch nicht vorhanden,
            //nach dem Update nochmal versuchen
            result = await _mediator.Send(new LaunchApplication.Command(settings.Mode));
            if (result.IsFailure)
            {
                AnsiConsole.MarkupLine($"[red]{result.Error!.Description}[/]");
                return -1;
            }
        }

        return 0;
    }
}
