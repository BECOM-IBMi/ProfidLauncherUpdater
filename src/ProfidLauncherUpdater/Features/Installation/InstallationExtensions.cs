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

            services.AddSingleton(installationConfig);

            return services;
        }
    }
}
