using System.Text.Json.Serialization;

namespace MikroTikSDN.Core.Models
{
    // O teu modelo principal (Server)
    public class DhcpModel
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("interface")] public string? Interface { get; set; }
        [JsonPropertyName("address-pool")] public string? AddressPool { get; set; }
        [JsonPropertyName("disabled")] public string? Disabled { get; set; }
    }

    // Modelo para o DHCP Client
    public class DhcpClientModel
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("interface")] public string? Interface { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("address")] public string? Address { get; set; }
        [JsonPropertyName("use-peer-dns")] public string? UsePeerDns { get; set; }
        [JsonPropertyName("add-default-route")] public string? AddDefaultRoute { get; set; }
        [JsonPropertyName("disabled")] public string? Disabled { get; set; }
    }
}