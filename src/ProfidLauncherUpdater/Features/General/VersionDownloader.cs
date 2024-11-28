using FlintSoft.Result;
using Microsoft.Extensions.Logging;
using ProfidLauncherUpdater.Shared;

namespace ProfidLauncherUpdater.Features.General;

public class VersionDownloader
{
    private readonly InstallationConfigurationModel _config;
    private readonly HttpClient _client;
    private readonly RemoteVersionService _remoteVersionService;
    private readonly LocalVersionService _localVersionService;
    private readonly ILogger<VersionDownloader> _logger;
    //private string _serverVersionFile = "";
    private string _downloadedFilePath = "";

    public string AppPath { get; set; }

    public VersionDownloader(InstallationConfigurationModel config,
        IHttpClientFactory httpClientFactory,
        RemoteVersionService remoteVersionService,
        LocalVersionService localVersionService,
        ILogger<VersionDownloader> logger)
    {
        _config = config;
        _client = httpClientFactory.CreateClient("repo");
        _remoteVersionService = remoteVersionService;
        _localVersionService = localVersionService;
        _logger = logger;

        AppPath = config.PathToApp;
        if (string.IsNullOrEmpty(config.PathToApp))
        {
            AppPath = Directory.GetCurrentDirectory();
        }
    }

    public async Task<Result<int>> DonwloadVersionFromServer(CancellationToken canellationToken, RepositoryModel? repo = null)
    {
        try
        {
            _logger.LogInformation($"Downloading version {repo?.VersionOnServer ?? "N/A"} from server...");

            var vToDownload = repo;
            if (vToDownload is null)
            {
                var res = await _remoteVersionService.GetCurrentVersionFromServer(canellationToken);
                if (res.IsFailure) return res.Error!.ToError();

                vToDownload = res.Value!;
            }

            var fileResult = await downloadFile(vToDownload!, canellationToken);
            if (fileResult.IsFailure) return fileResult.Error!.ToError();

            var writeResult = await writeFile(fileResult.Value!, canellationToken);
            if (writeResult.IsFailure) return writeResult.Error!.ToError();

            var target = Path.Combine(AppPath, $"v{vToDownload.VersionOnServer}");
            _logger.LogInformation($"Unzipping file into folder {target}...");
            var zipResult = await Tools.Unzipper(_downloadedFilePath, target, canellationToken);
            if (zipResult.IsFailure) return writeResult.Error!.ToError();

            var rmResult = await removeZipFile(canellationToken);
            if (rmResult.IsFailure) return writeResult.Error!.ToError();

            var newInfo = await _localVersionService.WriteInfo(vToDownload!.VersionOnServer!, canellationToken);
            if (newInfo.IsFailure) return newInfo.Error!.ToError();

            var oldVersions = await _localVersionService.RemoveOldVersions(canellationToken);
            if (oldVersions.IsFailure) return oldVersions.Error!.ToError();

            _logger.LogInformation("File downloaded!");
            return 1;
        }
        catch (Exception ex)
        {
            return new Error(nameof(VersionDownloader) + "." + nameof(DonwloadVersionFromServer) + ".Error", "Error loading a version file from server: " + ex.Message);
        }
    }

    private async Task<Result<(Stream serverFileStream, Asset? asset)>> downloadFile(RepositoryModel serverVersion, CancellationToken canellationToken)
    {
        try
        {
            var asset = serverVersion.ServerVersion?.assets?.FirstOrDefault();
            if (asset is null)
            {
                return new Error(nameof(downloadFile) + "_NO_ASSETS", "Couldn't find any release assets");
            }

            var client = new HttpClient()
            {
                BaseAddress = new Uri(asset.browser_download_url)
            };

            var resp = await client.GetAsync("");
            if (resp.IsSuccessStatusCode)
            {
                Stream streamToReadFrom = await resp.Content.ReadAsStreamAsync();
                if (streamToReadFrom is null)
                {
                    return new Error(nameof(downloadFile) + ".NotFound", "Current version package couldn't be found!");
                }

                return (streamToReadFrom, asset);
            }
            else
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new NotFound(nameof(downloadFile) + ".downloadUpdate", "Couldn't find the version on the server");
                }

                return new Error(nameof(downloadFile) + ".downloadUpdate", $"When downloading the latest version the server responded with {resp.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return new Error(nameof(VersionDownloader) + "." + nameof(downloadFile) + ".Error", "Couldn't load current version: " + ex.Message);
        }
    }

    private async Task<Result<bool>> writeFile((Stream fileStream, Asset? asset) serverAsset, CancellationToken canellationToken)
    {
        try
        {
            var downloadFolder = Path.Combine(AppPath, "tmp");
            _logger.LogInformation($"Writing downloaded file info folder {downloadFolder}...");

            if (!Directory.Exists(downloadFolder))
            {
                _logger.LogInformation($"Folder doesn't exist, need to create it...");
                Directory.CreateDirectory(downloadFolder);
            }

            if (string.IsNullOrEmpty(serverAsset.asset?.name))
            {
                return new Error(nameof(writeFile) + ".WRITE_FILE.NO_ASSET", "Asset file name from server is not availiable");
            }

            _downloadedFilePath = Path.Combine(downloadFolder, serverAsset.asset?.name ?? "");
            _logger.LogInformation($"Writing downloaded file into file {_downloadedFilePath}...");
            using FileStream outputFileStream = new(_downloadedFilePath, FileMode.CreateNew, FileAccess.Write);
            await serverAsset.fileStream.CopyToAsync(outputFileStream, canellationToken);

            await outputFileStream.FlushAsync();
            await outputFileStream.DisposeAsync();
            outputFileStream.Close();

            await serverAsset.fileStream.FlushAsync();
            await serverAsset.fileStream.DisposeAsync();
            serverAsset.fileStream.Close();

            return true;
        }
        catch (Exception ex)
        {
            return new Error(nameof(VersionDownloader) + "." + nameof(writeFile) + ".Error", "Couldn't write current version: " + ex.Message);
        }
    }

    private async Task<Result<bool>> removeZipFile(CancellationToken canellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting zip file...");
            await Task.Run(() => File.Delete(_downloadedFilePath), canellationToken);

            return true;
        }
        catch (Exception ex)
        {
            return new Error(nameof(VersionDownloader) + "." + nameof(removeZipFile) + ".Error", "Couldn't delete zip file: " + ex.Message);
        }
    }
}
