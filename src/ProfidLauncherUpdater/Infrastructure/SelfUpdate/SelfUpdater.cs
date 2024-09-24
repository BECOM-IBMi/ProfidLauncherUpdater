using FlintSoft.Result;
using FlintSoft.Result.Types;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net.Http.Json;

namespace ProfidLauncherUpdater.Infrastructure.SelfUpdate;

public class SelfUpdater
{
    private readonly IConfiguration _config;

    public SelfUpdater(IConfiguration config)
    {
        _config = config;


    }

    public async Task<Result<(string version, ServerVersionModel serverVersion)>> GetServerVersion()
    {
        try
        {
            var updaterReleaseUrl = _config.GetValue<string>("installation:repository:updater") ?? "";
            if (string.IsNullOrEmpty(updaterReleaseUrl))
            {
                return new Error("SERVER_VERSION_RELURL", "The release url is empty");
            }

            var client = new HttpClient
            {
                BaseAddress = new Uri(updaterReleaseUrl)
            };

            var serverRelease = await client.GetFromJsonAsync<ServerVersionModel>("");
            if (serverRelease is null)
            {
                return new Error("SERVER_VERSION_RESPONSE", "The version on the server is null");
            }

            //Die Version steht im Tag des Release
            //The tag consists of vXX.YY.ZZ
            var tag = serverRelease.tag_name;
            if (string.IsNullOrEmpty(tag))
            {
                return new Error("SERVER_VERSION_TAG", "The version tag from the server is empty");
            }

            var version = tag.Substring(1, tag.Length - 1);
            return (version, serverRelease);
        }
        catch (Exception ex)
        {
            return new Error("SERVER_VERSION_ERROR", $"When retrieving the server version the following error occured: {ex.Message}");
        }
    }

    public async Task<Result<Sucess>> UpdateSelf(ServerVersionModel serverVersion)
    {
        try
        {
            var updaterDownloadDirectory = _config.GetValue<string>("installation:repository:updaterDownloadDirectory") ?? "";
            if (string.IsNullOrEmpty(updaterDownloadDirectory))
            {
                return new Error("SELF_UPDATE_TARGET_DIR", "The download dir is empty");
            }

            var di = new DirectoryInfo(updaterDownloadDirectory);
            ensureDirectoryExists(di);

            var downloadResult = await downloadUpdate(serverVersion, updaterDownloadDirectory);
            if (downloadResult.IsFailure) return downloadResult.Error!.ToError();
            if (downloadResult.IsNotFound) return downloadResult.Error!.ToNotFound();

            runInstaller(downloadResult.Value!);
            return new Sucess();
        }
        catch (Exception ex)
        {
            return new Error("SELF_UPDATE_ERROR", $"When updating the updater, the following error occured: {ex.Message}");
        }
    }

    private void ensureDirectoryExists(DirectoryInfo directoryInfo)
    {
        if (directoryInfo.Exists)
        {
            directoryInfo.Delete(recursive: true);
        }
        directoryInfo.Create();
    }

    private async Task<Result<string>> downloadUpdate(ServerVersionModel serverVersion, string downloadDir)
    {
        try
        {
            var asset = serverVersion.assets.FirstOrDefault();
            if (asset is null)
            {
                return new Error("SELF_UPDATE_UPDATER_NO_ASSETS", "Couldn't find any release assets");
            }

            var client = new HttpClient()
            {
                BaseAddress = new Uri(asset.browser_download_url)
            };

            var targetFile = $@".\{downloadDir}\{asset.name}";

            var resp = await client.GetAsync("");

            if (resp.IsSuccessStatusCode)
            {
                using (Stream streamToReadFrom = await resp.Content.ReadAsStreamAsync())
                {
                    using (FileStream fileStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write))
                    {
                        await streamToReadFrom.CopyToAsync(fileStream);
                    }
                }

                var fi = new FileInfo(targetFile);
                if (fi.Exists)
                {
                    return fi.FullName;
                }
                else
                {
                    return new Error("SELF_UPDATE_UPDATER_NO_DOWNLOAD", $"After downloading the file {asset.name} is not persistet");
                }
            }
            else
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new NotFound("SELF_UPDATE_UPDATER_NOT_FOUND", "Couldn't find the version on the server");
                }

                return new Error("SELF_UPDATE_UPDATER_DNL_ERRPR", $"When downloading the latest version the server responded with {resp.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return new Error(ex, "SELF_UPDATE_UPDATER_ERRPR");
        }
    }

    void runInstaller(string msiFile)
    {
        var proc = new Process();

        proc.StartInfo.FileName = "msiexec";
        proc.StartInfo.Arguments = String.Format($"/i \"{msiFile}\" /passive");

        proc.Start();
    }
}















//    public async Task<Result<bool>> UpdateSelf(ServerVersionModel serverVersion)
//    {
//        try
//        {
//            var di = new DirectoryInfo($@".\{_config.Repository.UpdaterInfo.LocalDirectory}");
//            ensureDirectoryExists(di);

//            var downloadResult = await downloadUpdate(serverVersion);
//            if (downloadResult.IsFailure) return downloadResult.Error!.ToError();
//            if (downloadResult.IsNotFound) return downloadResult.Error!.ToNotFound();

//            var fi = new FileInfo(downloadResult.Value!);
//            var msiFile = Path.GetFileNameWithoutExtension(fi.FullName) + ".msi";
//            var msi = new FileInfo(Path.Combine(di.FullName, msiFile));

//            var unzipResult = await Tools.Unzipper(downloadResult.Value!, di.FullName);
//            if (unzipResult.IsFailure) return unzipResult.Error!.ToError();
//            if (unzipResult.IsNotFound) return unzipResult.Error!.ToNotFound();

//            runInstaller(msi.FullName);
//            return true;
//        }
//        catch (Exception)
//        {

//            throw;
//        }
//    }

//    private void ensureDirectoryExists(DirectoryInfo directoryInfo)
//    {
//        if (directoryInfo.Exists)
//        {
//            directoryInfo.Delete(recursive: true);
//        }
//        directoryInfo.Create();
//    }

//    private async Task<Result<string>> downloadUpdate(ServerVersionModel serverVersion)
//    {
//        try
//        {
//            var resp = await _client.GetAsync($"{_config.Repository.DownloadPath}{serverVersion.VersionId}");
//            if (resp.IsSuccessStatusCode)
//            {
//                using (Stream streamToReadFrom = await resp.Content.ReadAsStreamAsync())
//                {
//                    using (FileStream fileStream = new FileStream($@".\{_config.Repository.UpdaterInfo.LocalDirectory}\{serverVersion.FileName}", FileMode.Create, FileAccess.Write))
//                    {
//                        await streamToReadFrom.CopyToAsync(fileStream);
//                    }
//                }

//                var fi = new FileInfo($@".\{_config.Repository.UpdaterInfo.LocalDirectory}\{serverVersion.FileName}");
//                if (fi.Exists)
//                {
//                    return fi.FullName;
//                }
//                else
//                {
//                    return new Error("SelfUpdater.donwloadUpdate", $"After downloading the file {serverVersion.FileName} is not persistet");
//                }
//            }
//            else
//            {
//                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
//                {
//                    return new NotFound("SelfUpdater.donwloadUpdate", "Couldn't find the version on the server");
//                }

//                return new Error("SelfUpdater.downloadUpdate", $"When downloading the latest version the server responded with {resp.StatusCode}");
//            }
//        }
//        catch (Exception ex)
//        {
//            return new Error(ex, "SelfUpdater.downloadUpdate");
//        }
//    }

//    void runInstaller(string msiFile)
//    {
//        var proc = new Process();

//        proc.StartInfo.FileName = "msiexec";
//        proc.StartInfo.Arguments = String.Format($"/i \"{msiFile}\" /passive");

//        proc.Start();
//    }
//}
