using MediatR;
using ProfidLauncherUpdater.Features.Installation;
using ProfidLauncherUpdater.Shared;
using Spectre.Console;

namespace ProfidLauncherUpdater.Commands;

public class UpdateCommand(IMediator mediator) : AsyncCommand<UpdateCommand.Settings>
{
    private readonly IMediator _mediator = mediator;

    public class Settings : CommandSettings
    {

    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var result = await _mediator.Send(new Update.Command());
        if (result.IsFailure)
        {
            AnsiConsole.Markup($"[red]{result.Error.Description}[/]");
            return 0;
        }

        var msg = result.Value.state switch
        {
            InstallationState.UPTODATE => $"[lime]The local version is up-tp-date[/]",
            InstallationState.NEWINSTALLATION => $"[darkorange]First time installation -> New version is {result.Value.nVersion}[/]",
            InstallationState.UPDATED => $"[yellow]Version is updated from {result.Value.pVersion} to {result.Value.nVersion}[/]",
            _ => $"[purple]Unknown[/]"
        };

        AnsiConsole.Markup(msg);

        return 0;
    }
}
