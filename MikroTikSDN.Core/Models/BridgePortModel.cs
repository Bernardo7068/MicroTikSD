using System.Text.Json.Serialization;

namespace MikroTikSDN.Core.Models
{
    public class BridgePortModel
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("interface")] public string? Interface { get; set; }
        [JsonPropertyName("bridge")] public string? Bridge { get; set; }

        // ADICIONA ESTA LINHA:
        [JsonPropertyName("pvid")] public string? Pvid { get; set; }

        [JsonPropertyName("disabled")] public string? Disabled { get; set; }
        //[JsonPropertyName("inactive")] public string? Inactive { get; set; }
    }
}