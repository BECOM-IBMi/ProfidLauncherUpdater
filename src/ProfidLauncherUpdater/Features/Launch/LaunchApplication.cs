using MediatR;
using ProfidLauncherUpdater.Features.General;
using ProfidLauncherUpdater.Shared;
using System.Diagnostics;

namespace ProfidLauncherUpdater.Features.Launch
{
    public static class LaunchApplication
    {
        public record Command(string mode) : IRequest<Result<bool>>;

        internal sealed class Handler(LocalVersionService localVersionService,
            InstallationConfigurationModel config) : IRequestHandler<Command, Result<bool>>
        {
            private readonly LocalVersionService _localVersionService = localVersionService;
            private readonly InstallationConfigurationModel _config = config;

            public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var lvResult = await _localVersionService.GetLocalActiveVersion(cancellationToken);
                    if (lvResult.IsFailure) return lvResult.Error;

                    if (String.IsNullOrEmpty(lvResult.Value))
                    {
                        //Schaut so aus als ob es noch nicht gedownloaded wurde
                        return false;
                    }

                    var path = $"v{lvResult.Value}";
                    var appBasePath = Path.Combine(_config.PathToApp, path);
                    path = Path.Combine(appBasePath, _config.AppToLaunch);

                    //App starten
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = false;
                    startInfo.WorkingDirectory = appBasePath;
                    startInfo.FileName = path;
                    startInfo.Arguments = $"-m {request.mode} -u {_config.PathToApp}";
                    Process.Start(startInfo);


                    return true;
                }
                catch (Exception ex)
                {
                    return new Error(nameof(LaunchApplication) + "." + ".Error", "Error launching the application: " + ex.Message);
                }
            }
        }
    }
}
