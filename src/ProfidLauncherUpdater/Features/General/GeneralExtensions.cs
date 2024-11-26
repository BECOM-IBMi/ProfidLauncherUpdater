using Microsoft.Extensions.Configuration;

namespace ProfidLauncherUpdater.Features.General;

public static class GeneralExtensions
{
    public static IServiceCollection AddGeneral(this IServiceCollection services, IConfiguration config)
    {
        var repbase = config.GetValue<string>("installation:repository:repoBase") ?? "";
        services.AddHttpClient("repo", x =>
        {
            x.BaseAddress = new Uri(repbase);
            x.Timeout = TimeSpan.FromSeconds(30);
            x.DefaultRequestHeaders.UserAgent.ParseAdd("ProfidLauncherUpdater");
        });

        services.AddSingleton<LocalVersionService>();
        services.AddSingleton<RemoteVersionService>();
        services.AddSingleton<VersionDownloader>();

        return services;
    }
}
