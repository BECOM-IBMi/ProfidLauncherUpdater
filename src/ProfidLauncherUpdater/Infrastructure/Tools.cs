using System.Reflection;

namespace ProfidLauncherUpdater.Infrastructure;

public class Tools
{
    public static string GetCurrentVersion()
    {
        var version = "1.0.0+LOCALBUILD";
        var appAssembly = typeof(Program).Assembly;
        if (appAssembly != null)
        {
            var attrs = appAssembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute));
            if (attrs != null)
            {
                var infoVerAttr = (AssemblyInformationalVersionAttribute)attrs;
                if (infoVerAttr != null && infoVerAttr.InformationalVersion.Length > 6)
                {
                    version = infoVerAttr.InformationalVersion;
                }
            }
        }

        if (version.Contains('+'))
        {
            version = version[..version.IndexOf('+')];
        }
        return version;
    }

    public static void CheckForUpdate()
    {
        var webClient = new HttpClient
        {
            BaseAddress =
        };
    }
}
