using System.Text.Json.Serialization;

namespace MikroTikSDN.Core.Models
{
    public class IpPoolModel
    {
        // O ID interno do MikroTik (ex: *1)
        [JsonPropertyName(".id")]
        public string? Id { get; set; }

        // O nome da Pool (ex: dhcp_pool1)
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        // Os intervalos de IP (ex: 192.168.88.10-192.168.88.50)
        [JsonPropertyName("ranges")]
        public string? Ranges { get; set; }

        // Caso queiras encadear pools (Opcional)
        [JsonPropertyName("next-pool")]
        public string? NextPool { get; set; }
    }
}