using ProfidLauncherUpdater.Shared;
using System.IO.Compression;

namespace ProfidLauncherUpdater.Features.General;

public class VersionDownloader
{
    private readonly InstallationConfigurationModel _config;
    private readonly HttpClient _client;
    private readonly RemoteVersionService _remoteVersionService;
    private readonly LocalVersionService _localVersionService;
    private string _serverVersionFile = "";
    private string _downloadedFilePath = "";

    public string AppPath { get; set; }

    public VersionDownloader(InstallationConfigurationModel config,
        IHttpClientFactory httpClientFactory,
        RemoteVersionService remoteVersionService,
        LocalVersionService localVersionService)
    {
        _config = config;
        _client = httpClientFactory.CreateClient("repo");
        _remoteVersionService = remoteVersionService;
        _localVersionService = localVersionService;

        AppPath = config.PathToApp;
        if (string.IsNullOrEmpty(config.PathToApp))
        {
            AppPath = Directory.GetCurrentDirectory();
        }
    }

    public async Task<Result<int>> DonwloadVersionFromServer(string version = "")
    {
        try
        {
            var vToDownlaod = version;
            if (string.IsNullOrEmpty(vToDownlaod))
            {
                var res = await _remoteVersionService.GetCurrentVersionFromServer();
                if (res.IsFailure) return res.Error;

                vToDownlaod = res.Value;
            }
            _serverVersionFile = $"v{vToDownlaod}.zip";

            var fileResult = await downloadFile();
            if (fileResult.IsFailure) return fileResult.Error;

            var writeResult = await writeFile(fileResult.Value);
            if (writeResult.IsFailure) return writeResult.Error;

            var zipResult = await unzip();
            if (zipResult.IsFailure) return writeResult.Error;

            var rmResult = await removeZipFile();
            if (rmResult.IsFailure) return writeResult.Error;

            var newInfo = await _localVersionService.WriteInfo(vToDownlaod);
            if (newInfo.IsFailure) return newInfo.Error;

            var oldVersions = await _localVersionService.RemoveOldVersions();
            if (oldVersions.IsFailure) return oldVersions.Error;

            return 1;
        }
        catch (Exception ex)
        {
            return new Error(nameof(VersionDownloader) + "." + nameof(DonwloadVersionFromServer) + ".Error", "Error loading a version file from server: " + ex.Message);
        }
    }

    private async Task<Result<Stream>> downloadFile()
    {
        try
        {
            var stream = await _client.GetStreamAsync(_serverVersionFile);
            if (stream is null)
            {
                return new Error(nameof(downloadFile) + ".NotFound", "Current version package couldn't be found!");
            }

            return stream;
        }
        catch (Exception ex)
        {
            return new Error(nameof(VersionDownloader) + "." + nameof(downloadFile) + ".Error", "Couldn't load current version: " + ex.Message);
        }
    }

    private async Task<Result<bool>> writeFile(Stream fileStream)
    {
        try
        {
            var downloadFolder = Path.Combine(AppPath, "tmp");
            if (!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
            }

            _downloadedFilePath = Path.Combine(downloadFolder, _serverVersionFile);
            using FileStream outputFileStream = new(_downloadedFilePath, FileMode.CreateNew);
            await fileStream.CopyToAsync(outputFileStream);

            return true;
        }
        catch (Exception ex)
        {
            return new Error(nameof(VersionDownloader) + "." + nameof(writeFile) + ".Error", "Couldn't write current version: " + ex.Message);
        }
    }

    private async Task<Result<bool>> unzip()
    {
        try
        {
            await Task.Run(() => ZipFile.ExtractToDirectory(_downloadedFilePath, AppPath));

            return true;
        }
        catch (Exception ex)
        {
            return new Error(nameof(VersionDownloader) + "." + nameof(unzip) + ".Error", "Error unzipping version file: " + ex.Message);
        }
    }

    private async Task<Result<bool>> removeZipFile()
    {
        try
        {
            await Task.Run(() => File.Delete(_downloadedFilePath));

            return true;
        }
        catch (Exception ex)
        {
            return new Error(nameof(VersionDownloader) + "." + nameof(removeZipFile) + ".Error", "Couldn't delete zip file: " + ex.Message);
        }
    }
}
