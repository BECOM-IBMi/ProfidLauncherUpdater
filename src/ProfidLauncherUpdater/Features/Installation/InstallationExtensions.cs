using Microsoft.Extensions.Configuration;
using ProfidLauncherUpdater.Shared;

namespace ProfidLauncherUpdater.Features.Installation
{
    public static class InstallationExtensions
    {
        public static IServiceCollection AddCurrentInstallationFeature(this IServiceCollection services, IConfiguration configuration)
        {
            var installationConfig = new InstallationConfigurationModel();
            configuration.GetSection("installation").Bind(installationConfig);

            var updater = new UpdaterInfo();
            configuration.GetSection("installation:repository:updater").Bind(updater);

            var launcher = new LauncherInfo();
            configuration.GetSection("installation:repository:launcher").Bind(launcher);

            installationConfig.Repository.UpdaterInfo = updater;
            installationConfig.Repository.LauncherInfo = launcher;

            services.AddSingleton(installationConfig);

            return services;
        }
    }
}
