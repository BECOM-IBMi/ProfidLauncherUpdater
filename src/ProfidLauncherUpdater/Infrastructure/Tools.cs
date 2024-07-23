using FlintSoft.Result;
using ProfidLauncherUpdater.Features.General;
using System.IO.Compression;
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

    public static async Task<Result<bool>> Unzipper(string zippedFile, string target, CancellationToken canellationToken = default)
    {
        try
        {
            await Task.Run(() => ZipFile.ExtractToDirectory(zippedFile, target), canellationToken);

            return true;
        }
        catch (Exception ex)
        {
            return new Error(nameof(VersionDownloader) + "." + nameof(Unzipper) + ".Error", "Error unzipping version file: " + ex.Message);
        }
    }
}
