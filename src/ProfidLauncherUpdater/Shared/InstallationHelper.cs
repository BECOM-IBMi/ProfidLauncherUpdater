using System.IO.Compression;
using System.Text.Json;

namespace ProfidLauncherUpdater.Shared;

public class InstallationHelper
{
    public static Result<DirectoryInfo[]> GetFoldersInBaseDirectory(InstallationConfigurationModel config)
    {
        var baseFolder = config.PathToApp;
        if (string.IsNullOrEmpty(baseFolder))
        {
            baseFolder = Directory.GetCurrentDirectory();
        }

        DirectoryInfo info = new(baseFolder);
        if (!info.Exists)
        {
            //Fehler
            return new Error(nameof(GetFoldersInBaseDirectory) + ".ReadFolders", "No base folder");
        }

        return info.GetDirectories();
    }

    public static Result<string[]?> GetLocalVersions(InstallationConfigurationModel config)
    {
        var foldersResult = GetFoldersInBaseDirectory(config);
        if (foldersResult.IsFailure) return foldersResult.Error;

        return GetLocalVersions(foldersResult.Value);
    }

    public static Result<string[]?> GetLocalVersions(IEnumerable<DirectoryInfo> folders)
    {
        var vFolders = folders.Where(x => x.Name.StartsWith('v'));
        if (!vFolders.Any())
        {
            //Erstinstallation
            return Result<string[]?>.Success(null);
        }

        return folders.Select(x => x.Name).ToArray();
    }

    public static async Task<Result<InfoModel?>> LoadInfo(string configFilePath)
    {
        try
        {
            //Json File laden, wenn es dieses gibt
            if (!File.Exists(configFilePath))
            {
                //Ist eine Neu-Installation
                return Result<InfoModel?>.Success(null);
            }

            using StreamReader reader = new(configFilePath);
            var json = await reader.ReadToEndAsync();

            var info = JsonSerializer.Deserialize<InfoModel>(json) ?? throw new Exception("Couldn't load info json file");
            return info;
        }
        catch (Exception ex)
        {
            return new Error(nameof(GetCurrentVersionFromServer) + ".Error", "Error loading info json: " + ex.Message);
        }
    }

    /// <summary>
    /// Loads the current info json file if it exists and returns the local active version
    /// </summary>
    /// <param name="configFilePath"></param>
    /// <returns>String result (f.e.: 2.0.3)</returns>
    public static async Task<Result<string?>> GetLocalActiveVersion(string configFilePath)
    {
        try
        {
            var result = await LoadInfo(configFilePath);
            if (result.IsFailure) return result.Error;

            if (result.Value is null) return "";

            return result.Value.Active;
        }
        catch (Exception ex)
        {
            return new Error(nameof(GetCurrentVersionFromServer) + ".Error", "Error loading info json: " + ex.Message);
        }
    }

    /// <summary>
    /// Downloads the version info from the server
    /// </summary>
    /// <param name="path">Url string to the file</param>
    /// <returns>String result (f.e.: 2.0.3)</returns>
    public static async Task<Result<string>> GetCurrentVersionFromServer(string path)
    {
        try
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(path)
            };

            var response = await client.GetAsync("");
            if (!response.IsSuccessStatusCode)
            {
                return new Error(nameof(GetCurrentVersionFromServer) + ".StatusCode", "Couldn't load version from repository: " + response.StatusCode);
            }

            var repoStream = await response.Content.ReadAsStreamAsync();
            var repo = await JsonSerializer.DeserializeAsync<RepositoryModel>(repoStream);

            if (repo is null)
            {
                return new Error(nameof(GetCurrentVersionFromServer) + ".JSON", "Couldn't load json");
            }

            return repo.Current;
        }
        catch (Exception ex)
        {
            return new Error(nameof(GetCurrentVersionFromServer) + ".Error", "Error loading verison: " + ex.Message);
        }
    }

    public static async Task<Result<bool>> DownloadCurrentVersionFromServer(string serverFilePath, string zipFilePath, string basePath)
    {
        var fileResult = await downloadFile(serverFilePath);
        if (fileResult.IsFailure) return fileResult.Error;

        var tmpFolder = Path.GetDirectoryName(zipFilePath);
        if (tmpFolder is not null && !Directory.Exists(tmpFolder))
        {
            Directory.CreateDirectory(tmpFolder);
        }

        var writeResult = await writeFile(fileResult.Value, zipFilePath);
        if (writeResult.IsFailure) return writeResult.Error;

        var zipResult = await unzip(zipFilePath, basePath);
        if (zipResult.IsFailure) return writeResult.Error;

        var rmResult = await removeZipFile(zipFilePath);
        if (rmResult.IsFailure) return writeResult.Error;



        return true;
    }

    private static async Task<Result<Stream>> downloadFile(string path)
    {
        var client = new HttpClient()
        {
            BaseAddress = new Uri(path)
        };

        try
        {
            var stream = await client.GetStreamAsync(path);
            if (stream is null)
            {
                return new Error(nameof(downloadFile) + ".NotFound", "Current version package couldn't be found!");
            }

            return stream;
        }
        catch (Exception ex)
        {
            return new Error(nameof(downloadFile) + ".Error", "Couldn't load current version: " + ex.Message);
        }
    }

    private static async Task<Result<bool>> writeFile(Stream fileStream, string targetFile)
    {
        try
        {
            using FileStream outputFileStream = new(targetFile, FileMode.CreateNew);
            await fileStream.CopyToAsync(outputFileStream);

            return true;
        }
        catch (Exception ex)
        {
            return new Error(nameof(writeFile) + ".Error", "Couldn't write current version: " + ex.Message);
        }
    }

    private static async Task<Result<bool>> unzip(string zipFile, string unzipTo)
    {
        try
        {
            await Task.Run(() => ZipFile.ExtractToDirectory(zipFile, unzipTo));

            return true;
        }
        catch (Exception ex)
        {
            return new Error(nameof(unzip) + ".Error", "Couldn't unzip current version: " + ex.Message);
        }
    }

    private static async Task<Result<bool>> removeZipFile(string zipFile)
    {
        try
        {
            await Task.Run(() => File.Delete(zipFile));

            return true;
        }
        catch (Exception ex)
        {
            return new Error(nameof(unzip) + ".Error", "Couldn't delete zip file: " + ex.Message);
        }
    }


}
