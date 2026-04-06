using System.Text.Json.Serialization;

namespace MikroTikSDN.Core.Models
{
    public class WireGuardInterfaceModel
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("listen-port")] public string? ListenPort { get; set; }
        [JsonPropertyName("public-key")] public string? PublicKey { get; set; }
        [JsonPropertyName("private-key")] public string? PrivateKey { get; set; }
        [JsonPropertyName("disabled")] public string? Disabled { get; set; }
        [JsonPropertyName("running")] public string? Running { get; set; }
        [JsonPropertyName("mtu")] public string? Mtu { get; set; }
    }

    public class WireGuardPeerModel
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("interface")] public string? Interface { get; set; }
        // Propriedade local para a nossa App guardar a chave
        [JsonPropertyName("private-key")] public string PrivateKey { get; set; }
        [JsonPropertyName("public-key")] public string? PublicKey { get; set; }
        [JsonPropertyName("allowed-address")] public string? AllowedAddress { get; set; }
        [JsonPropertyName("comment")] public string? Comment { get; set; }
        [JsonPropertyName("disabled")] public string? Disabled { get; set; }
    }

    // O NOVO MODELO PARA LER O DNS CLOUD
    public class MikroTikCloudModel
    {
        [JsonPropertyName("ddns-enabled")] public string? DdnsEnabled { get; set; }
        [JsonPropertyName("dns-name")] public string? DnsName { get; set; }
        [JsonPropertyName("public-address")] public string? PublicAddress { get; set; }
    }
}