using FlintSoft.Result;
using MediatR;
using ProfidLauncherUpdater.Features.General;
using ProfidLauncherUpdater.Shared;

namespace ProfidLauncherUpdater.Features.Installation;

public static class Update
{
    public record Command : IRequest<Result<(InstallationState state, string pVersion, string nVersion)>>;

    internal sealed class Handler(RemoteVersionService remoteVersionService,
        LocalVersionService localVersionService,
        VersionDownloader versionDownloader) : IRequestHandler<Command, Result<(InstallationState state, string pVersion, string nVersion)>>
    {
        private readonly RemoteVersionService _remoteVersionService = remoteVersionService;
        private readonly LocalVersionService _localVersionService = localVersionService;
        private readonly VersionDownloader _versionDownloader = versionDownloader;

        public async Task<Result<(InstallationState state, string pVersion, string nVersion)>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                //Read active local version
                var lversion = await _localVersionService.GetLocalActiveVersion(cancellationToken);
                if (lversion.IsFailure) return lversion.Error!.ToError();

                //Read latest from server
                var currentServerVersionResult = await _remoteVersionService.GetCurrentVersionFromServer(cancellationToken);
                if (currentServerVersionResult.IsFailure) return currentServerVersionResult.Error!.ToError();

                //Check current version
                var lvResult = await _localVersionService.GetLocalVersions(cancellationToken);
                if (lvResult.IsFailure) return lvResult.Error!.ToError();

                if (lvResult is null || lvResult.Value is null || !lvResult.Value.Any())
                {
                    //firsttime -> download latest
                    await _versionDownloader.DonwloadVersionFromServer(cancellationToken);

                    return (InstallationState.NEWINSTALLATION, "", currentServerVersionResult.Value!.VersionOnServer ?? "N/A");
                }

                if (lvResult.Value.Any(x => x == currentServerVersionResult.Value!.VersionOnServer))
                {
                    if (lversion.Value == "")
                    {
                        //Aus irgendeinemgrund wurde keine version ins info file geschrieben
                        var info = await _localVersionService.WriteInfo(currentServerVersionResult.Value!.VersionOnServer ?? "N/A", cancellationToken);
                        if (info.IsFailure) return info.Error!.ToError();
                    }

                    //same -> do nothing
                    return (InstallationState.UPTODATE, "", "");
                }

                //if newer -> download latest
                await _versionDownloader.DonwloadVersionFromServer(cancellationToken);

                return (InstallationState.UPDATED, lversion.Value!, currentServerVersionResult.Value!.VersionOnServer!);
            }
            catch (Exception ex)
            {
                return new Error(nameof(Update) + "." + ".Error", "Error updating the application: " + ex.Message);
            }
        }
    }
}
