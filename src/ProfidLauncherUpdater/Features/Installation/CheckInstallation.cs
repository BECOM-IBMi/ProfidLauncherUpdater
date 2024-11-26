using FlintSoft.Result;
using MediatR;
using ProfidLauncherUpdater.Features.General;
using ProfidLauncherUpdater.Shared;

namespace ProfidLauncherUpdater.Features.Installation;

public static class CheckInstallation
{
    public record Command : IRequest<Result<InstallationState>>;

    internal sealed class Handler(InstallationConfigurationModel configuration, LocalVersionService localVersionService, RemoteVersionService remoteVersionService) : IRequestHandler<Command, Result<InstallationState>>
    {
        private readonly InstallationConfigurationModel _configuration = configuration;
        private readonly LocalVersionService _localVersionService = localVersionService;
        private readonly RemoteVersionService _remoteVersionService = remoteVersionService;

        public async Task<Result<InstallationState>> Handle(Command request, CancellationToken cancellationToken)
        {
            ////Uns intressieren nur die vx.y.z 
            var fResult = await _localVersionService.GetLocalVersions(cancellationToken);
            if (fResult.IsFailure)
            {
                return fResult.Error!.ToError();
            }
            if (!fResult.Value!.Any())
            {
                //Erstinstallation
                return InstallationState.NEWINSTALLATION;
            }

            //Versionsordner vorhanden, prüfen
            //Aktuelle Version holen
            var currentServerVersionResult = await _remoteVersionService.GetCurrentVersionFromServer(cancellationToken);
            if (currentServerVersionResult.IsFailure) return currentServerVersionResult.Error!.ToError();

            if (!fResult.Value!.Any(x => x == currentServerVersionResult.Value!.VersionOnServer))
            {
                //Aktueller Ordner nicht vorhanden
                return InstallationState.NEEDUPDATE;
            }

            return InstallationState.UPTODATE;
        }
    }
}
