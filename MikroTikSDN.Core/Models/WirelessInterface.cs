using System.Text.Json.Serialization;

namespace MikroTikSDN.Core.Models
{
    public class WirelessInterface
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("ssid")] public string? Ssid { get; set; }
        [JsonPropertyName("security-profile")] public string? SecurityProfile { get; set; }
        [JsonPropertyName("disabled")] public string? Disabled { get; set; }
        [JsonPropertyName("running")] public string? Running { get; set; }
        [JsonPropertyName("mode")] public string? Mode { get; set; }
        [JsonPropertyName("band")] public string? Band { get; set; }
        [JsonPropertyName("frequency")] public string? Frequency { get; set; }
    }
}