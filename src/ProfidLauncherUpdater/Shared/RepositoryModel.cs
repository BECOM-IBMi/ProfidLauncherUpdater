using System.Text.Json.Serialization;

namespace ProfidLauncherUpdater.Shared;

public class RepositoryModel
{
    [JsonPropertyName("id")]
    public string VersionId { get; set; } = "";

    [JsonPropertyName("version")]
    public string LatestVersion { get; set; } = "";

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = "";
}
