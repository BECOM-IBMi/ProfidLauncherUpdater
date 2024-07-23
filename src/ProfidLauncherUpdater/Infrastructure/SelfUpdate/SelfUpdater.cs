using FlintSoft.Result;
using Microsoft.Extensions.Configuration;
using ProfidLauncherUpdater.Shared;
using System.Diagnostics;
using System.Net.Http.Json;

namespace ProfidLauncherUpdater.Infrastructure.SelfUpdate;

public class SelfUpdater
{
    private readonly RepositoryConfigurationModel _config;
    private readonly HttpClient _client;

    public SelfUpdater(IConfiguration config)
    {
        var cfg = new RepositoryConfigurationModel();
        config.GetSection("installation:repository").Bind(cfg);
        _config = cfg;

        _client = new HttpClient
        {
            BaseAddress = new Uri(_config.BasePath)
        };
    }

    public async Task<Result<ServerVersionModel>> GetServerVersion()
    {
        try
        {
            var serverVersion = await _client.GetFromJsonAsync<ServerVersionModel>($"{_config.VersionPath}{_config.SoftwareId}");
            if (serverVersion is null)
            {
                return new Error("SERVER_VERSION", "The version on the server is null");
            }
            return serverVersion;
        }
        catch (Exception ex)
        {
            return new Error("SERVER_VERSION", $"When retrieving the server version the following error occured: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateSelf(ServerVersionModel serverVersion)
    {
        try
        {
            var di = new DirectoryInfo($@".\{_config.LocalDirectory}");
            ensureDirectoryExists(di);

            var downloadResult = await downloadUpdate(serverVersion);
            if (downloadResult.IsFailure) return downloadResult.Error!.ToError();
            if (downloadResult.IsNotFound) return downloadResult.Error!.ToNotFound();

            var fi = new FileInfo(downloadResult.Value!);
            var msiFile = Path.GetFileNameWithoutExtension(fi.FullName) + ".msi";
            var msi = new FileInfo(Path.Combine(di.FullName, msiFile));

            var unzipResult = await Tools.Unzipper(downloadResult.Value!, di.FullName);
            if (unzipResult.IsFailure) return unzipResult.Error!.ToError();
            if (unzipResult.IsNotFound) return unzipResult.Error!.ToNotFound();

            runInstaller(msi.FullName);
            return true;
        }
        catch (Exception)
        {

            throw;
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

    private async Task<Result<string>> downloadUpdate(ServerVersionModel serverVersion)
    {
        try
        {
            var resp = await _client.GetAsync($"{_config.DownloadPath}{serverVersion.VersionId}");
            if (resp.IsSuccessStatusCode)
            {
                using (Stream streamToReadFrom = await resp.Content.ReadAsStreamAsync())
                {
                    using (FileStream fileStream = new FileStream($@".\{_config.LocalDirectory}\{serverVersion.FileName}", FileMode.Create, FileAccess.Write))
                    {
                        await streamToReadFrom.CopyToAsync(fileStream);
                    }
                }

                var fi = new FileInfo($@".\{_config.LocalDirectory}\{serverVersion.FileName}");
                if (fi.Exists)
                {
                    return fi.FullName;
                }
                else
                {
                    return new Error("SelfUpdater.donwloadUpdate", $"After downloading the file {serverVersion.FileName} is not persistet");
                }
            }
            else
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new NotFound("SelfUpdater.donwloadUpdate", "Couldn't find the version on the server");
                }

                return new Error("SelfUpdater.downloadUpdate", $"When downloading the latest version the server responded with {resp.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return new Error(ex, "SelfUpdater.downloadUpdate");
        }
    }

    void runInstaller(string msiFile)
    {
        var proc = new Process();
        proc.StartInfo.FileName = "msiexec";
        proc.StartInfo.Arguments = String.Format($"/i {msiFile} /quiet /qn");
        proc.Start();
    }
}
