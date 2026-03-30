using System.Text.Json.Serialization;

namespace MikroTikSDN.Core.Models
{
    public class NetworkInterface
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("mac-address")] public string? MacAddress { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("running")] public bool Running { get; set; }
        [JsonPropertyName("disabled")] public bool Disabled { get; set; }
        [JsonPropertyName("comment")] public string? Comment { get; set; }
        [JsonPropertyName("mtu")] public string? Mtu { get; set; }
    }
}