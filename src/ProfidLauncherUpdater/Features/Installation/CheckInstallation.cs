using MediatR;
using ProfidLauncherUpdater.Shared;

namespace ProfidLauncherUpdater.Features.Installation;

public static class CheckInstallation
{
    public record Command : IRequest<Result<InstallationState>>;

    internal sealed class Handler(InstallationConfigurationModel configuration) : IRequestHandler<Command, Result<InstallationState>>
    {
        private readonly InstallationConfigurationModel _configuration = configuration;

        public async Task<Result<InstallationState>> Handle(Command request, CancellationToken cancellationToken)
        {
            //Welche Ordner gibt es 
            var foldersResult = InstallationHelper.GetFoldersInBaseDirectory(_configuration);
            if (foldersResult.IsFailure) return foldersResult.Error;

            //Uns intressieren nur die vx.y.z 
            var fResult = InstallationHelper.GetLocalVersions(foldersResult.Value);
            if (fResult.Value is null)
            {
                //Erstinstallation
                return InstallationState.NEWINSTALLATION;
            }

            //Versionsordner vorhanden, prüfen
            //Aktuelle Version holen
            var vResult = await InstallationHelper.GetCurrentVersionFromServer($"{_configuration.Repository.BasePath}{_configuration.Repository.VersionFile}");
            if (vResult.IsFailure) return vResult.Error;

            var cVersion = "v" + vResult.Value;
            if (fResult.Value.Any(x => x == cVersion))
            {
                //Aktueller Ordner nicht vorhanden
                return InstallationState.NEEDUPDATE;
            }

            return InstallationState.UPTODATE;
        }
    }
}
