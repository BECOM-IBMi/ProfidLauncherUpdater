using System.Text.Json.Serialization;

namespace ProfidLauncherUpdater.Shared
{
    public class InfoModel
    {
        [JsonPropertyName("active")]
        public string Active { get; set; } = "";

        [JsonPropertyName("previous")]
        public string Previous { get; set; } = "";
    }
}
