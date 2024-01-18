using System.Text.Json.Serialization;

namespace ProfidLauncherUpdater.Shared;

public class RepositoryModel
{
    [JsonPropertyName("current")]
    public string Current { get; set; } = "";
}
