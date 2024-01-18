using Microsoft.Extensions.Configuration;
using ProfidLauncherUpdater.Features.General;
using ProfidLauncherUpdater.Features.Installation;

namespace ProfidLauncherUpdater.Features;

public static class FeaturesExtensions
{
    public static IServiceCollection AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGeneral(configuration);
        services.AddCurrentInstallationFeature(configuration);

        services.AddMediatR(opt => opt.RegisterServicesFromAssembly(typeof(Program).Assembly));

        return services;
    }
}
