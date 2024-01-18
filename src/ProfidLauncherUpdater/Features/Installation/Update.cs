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
                var lversion = await _localVersionService.GetLocalActiveVersion();
                if (lversion.IsFailure) return lversion.Error;

                //Read latest from server
                var vResult = await _remoteVersionService.GetCurrentVersionFromServer();
                if (vResult.IsFailure) return vResult.Error;

                //Check current version
                var lvResult = await _localVersionService.GetLocalVersions();
                if (lvResult.IsFailure) return lvResult.Error;

                if (lvResult is null || lvResult.Value is null || !lvResult.Value.Any())
                {
                    //firsttime -> download latest
                    await _versionDownloader.DonwloadVersionFromServer();

                    return (InstallationState.NEWINSTALLATION, "", vResult.Value);
                }

                var cVersion = vResult.Value;
                if (lvResult.Value.Any(x => x == cVersion))
                {
                    if (lversion.Value == "")
                    {
                        //Aus irgendeinemgrund wurde keine version ins info file geschrieben
                        var info = await _localVersionService.WriteInfo(cVersion);
                        if (info.IsFailure) return info.Error;
                    }

                    //same -> do nothing
                    return (InstallationState.UPTODATE, "", "");
                }

                //if newer -> download latest
                await _versionDownloader.DonwloadVersionFromServer();

                return (InstallationState.UPDATED, lversion.Value, vResult.Value);
            }
            catch (Exception ex)
            {
                return new Error(nameof(Update) + "." + ".Error", "Error updating the application: " + ex.Message);
            }
        }
    }
}
