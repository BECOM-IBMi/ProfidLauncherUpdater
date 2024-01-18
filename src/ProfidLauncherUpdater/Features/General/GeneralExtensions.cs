using Microsoft.Extensions.Configuration;

namespace ProfidLauncherUpdater.Features.General;

public static class GeneralExtensions
{
    public static IServiceCollection AddGeneral(this IServiceCollection services, IConfiguration config)
    {
        var repbase = config.GetValue<string>("installation:repository:basePath") ?? "";
        services.AddHttpClient("repo", x =>
        {
            x.BaseAddress = new Uri(repbase);
        });

        services.AddScoped<LocalVersionService>();
        services.AddScoped<RemoteVersionService>();
        services.AddScoped<VersionDownloader>();

        return services;
    }
}
