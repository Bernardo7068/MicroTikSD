using System.Text.Json.Serialization;

namespace MikroTikSDN.Core.Models
{
    public class IpAddressModel
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("address")] public string? Address { get; set; }
        [JsonPropertyName("network")] public string? Network { get; set; }
        [JsonPropertyName("interface")] public string? Interface { get; set; }
        [JsonPropertyName("dynamic")] public string? Dynamic { get; set; }
        [JsonPropertyName("disabled")] public string? Disabled { get; set; }
    }
}