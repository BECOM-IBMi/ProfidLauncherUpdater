
using MediatR;
using ProfidLauncherUpdater.Features.Installation;
using ProfidLauncherUpdater.Shared;
using Spectre.Console;

namespace ProfidLauncherUpdater.Commands;

public class CheckInstallationCommand(IMediator mediator) : AsyncCommand<CheckInstallationCommand.Settings>
{
    private readonly IMediator _mediator = mediator;

    public class Settings : CommandSettings
    {

    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var result = await _mediator.Send(new CheckInstallation.Command());
        if (result.IsFailure)
        {
            AnsiConsole.Markup($"[red]{result.Error!.Description}[/]");
        }

        var msg = result.Value switch
        {
            InstallationState.NEWINSTALLATION => $"[yellow]This is a new installation[/]",
            InstallationState.NEEDUPDATE => $"[darkorange]The current installation is not up to date[/]",
            InstallationState.UPTODATE => $"[lime]The installation is up to date[/]",
            _ => $"[purple]Unknown[/]"
        };

        AnsiConsole.Markup(msg);

        return 0;
    }
}
