using System.Text.Json.Serialization;

namespace ProfidLauncherUpdater.Infrastructure.SelfUpdate;

public class ServerVersionModel
{
    [JsonPropertyName("id")]
    public Guid VersionId { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("filename")]
    public string FileName { get; set; } = "";
}
