using System.Text.Json.Serialization;

namespace ProfidLauncherUpdater.Shared;

public class InstallationConfigurationModel
{
    [JsonPropertyName("appToLaunch")]
    public string AppToLaunch { get; set; } = "";

    [JsonPropertyName("pathToApp")]
    public string PathToApp { get; set; } = "";

    [JsonPropertyName("localInfo")]
    public string InfoFileName { get; set; } = "";

    [JsonPropertyName("repository")]
    public RepositoryConfigurationModel Repository { get; set; } = new();
}

public class RepositoryConfigurationModel
{
    [JsonPropertyName("basePath")]
    public string BasePath { get; set; } = "";

    [JsonPropertyName("versionPath")]
    public string VersionPath { get; set; } = "";

    [JsonPropertyName("downloadPath")]
    public string DownloadPath { get; set; } = "";

    [JsonPropertyName("softwareId")]
    public string SoftwareId { get; set; } = "";

    [JsonPropertyName("localDirectory")]
    public string LocalDirectory { get; set; } = "";

    public string VersionFile { get; set; } = "";
}