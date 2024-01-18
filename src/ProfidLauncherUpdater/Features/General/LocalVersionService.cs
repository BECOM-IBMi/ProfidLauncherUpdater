using ProfidLauncherUpdater.Shared;
using System.Text.Json;

namespace ProfidLauncherUpdater.Features.General;

public class LocalVersionService
{
    private readonly InstallationConfigurationModel _config;
    private InfoModel? _info;
    private List<DirectoryInfo>? _dirsInApp;

    public string AppPath { get; set; }
    public string InfoFilePath => Path.Combine(_config.PathToApp, _config.InfoFileName);

    public LocalVersionService(InstallationConfigurationModel config)
    {
        _config = config;

        AppPath = config.PathToApp;
        if (string.IsNullOrEmpty(config.PathToApp))
        {
            AppPath = Directory.GetCurrentDirectory();
        }
    }

    public async Task<Result<InfoModel>> LoadInfo()
    {
        try
        {
            if (_info is null)
            {
                //Json File laden, wenn es dieses gibt
                if (!File.Exists(InfoFilePath))
                {
                    //Ist eine Neu-Installation
                    _info = new InfoModel();
                    return _info;
                }

                using StreamReader reader = new(InfoFilePath);
                var json = await reader.ReadToEndAsync();

                var info = JsonSerializer.Deserialize<InfoModel>(json) ?? throw new Exception("Couldn't load info json file");
                _info = info;
            }

            return _info;
        }
        catch (Exception ex)
        {
            return new Error(nameof(LocalVersionService) + "." + nameof(LoadInfo) + ".Error", "Error loading info json: " + ex.Message);
        }
    }

    public async Task<Result<bool>> WriteInfo(string newVersion)
    {
        try
        {
            var currentInfo = await LoadInfo();
            if (currentInfo.IsFailure) return currentInfo.Error;

            if (File.Exists(InfoFilePath))
            {
                File.Delete(InfoFilePath);
            }

            _info!.Previous = _info!.Active;
            _info!.Active = newVersion;

            var json = JsonSerializer.Serialize(_info);

            await File.WriteAllTextAsync(InfoFilePath, json);

            return true;
        }
        catch (Exception ex)
        {
            return new Error(nameof(LocalVersionService) + "." + nameof(WriteInfo) + ".Error", "Error writing new info file: " + ex.Message);
        }
    }

    public async Task<Result<List<DirectoryInfo>>> GetFoldersInBaseDirectory()
    {
        try
        {
            return await Task.Run<Result<List<DirectoryInfo>>>(() =>
            {
                if (_dirsInApp is null)
                {
                    DirectoryInfo info = new(AppPath);
                    if (!info.Exists)
                    {
                        //Fehler
                        return new Error(nameof(GetFoldersInBaseDirectory) + ".ReadFolders", "No base folder");
                    }

                    var directories = info.GetDirectories().ToList();

                    _dirsInApp = directories;
                }

                return _dirsInApp;
            });
        }
        catch (Exception ex)
        {
            return new Error(nameof(LocalVersionService) + "." + nameof(GetLocalVersions) + ".Error", "Error local versions: " + ex.Message);
        }
    }

    public async Task<Result<string>> GetLocalActiveVersion()
    {
        try
        {
            var info = await LoadInfo();
            if (info.IsFailure) return info.Error;

            return info.Value.Active;
        }
        catch (Exception ex)
        {
            return new Error(nameof(LocalVersionService) + "." + nameof(GetLocalActiveVersion) + ".Error", "Error loading local active version: " + ex.Message);
        }
    }

    public async Task<Result<List<string>>> GetLocalVersions()
    {
        try
        {
            var dirs = await GetFoldersInBaseDirectory();
            if (dirs.IsFailure) return dirs.Error;

            var versions = dirs.Value.Where(x => x.Name.StartsWith('v')).Select(x => x.Name.Remove(0, 1)).ToList();
            if (versions is null) return new Error(nameof(LocalVersionService) + "." + nameof(GetLocalVersions) + ".READ_VERSIONS", "Couldn't read versions from the local folders");

            return versions;

        }
        catch (Exception ex)
        {
            return new Error(nameof(LocalVersionService) + "." + nameof(GetLocalVersions) + ".Error", "Error local versions: " + ex.Message);
        }
    }

    public async Task<Result<bool>> RemoveOldVersions()
    {
        try
        {
            var dirs = await GetFoldersInBaseDirectory();
            if (dirs.IsFailure) return dirs.Error;

            var info = await LoadInfo();
            if (info.IsFailure) return info.Error;

            foreach (var dir in dirs.Value)
            {
                //Prüfen ob es e nicht die aktuelle oder die vorherige Version ist
                if (dir.Name == $"v{info.Value.Active}" || dir.Name == $"v{info.Value.Previous}")
                {
                    continue;
                }

                //Dies ist ein altes dir, das kann gelöscht werden
                dir.Delete();
            }

            return true;
        }
        catch (Exception ex)
        {
            return new Error(nameof(LocalVersionService) + "." + nameof(RemoveOldVersions) + ".Error", "Error local versions: " + ex.Message);
        }
    }
}
