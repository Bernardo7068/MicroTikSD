using System.Text.Json.Serialization;

namespace MikroTikSDN.Core.Models
{
    public class DnsSettingsModel
    {
        [JsonPropertyName("servers")] public string? Servers { get; set; }

        [JsonPropertyName("dynamic-servers")] public string? DynamicServers { get; set; }

        [JsonPropertyName("use-doh-server")] public string? UseDohServer { get; set; }

        [JsonPropertyName("mdns-repeater-interfaces")] public string? MdnsRepeaterInterfaces { get; set; }

        [JsonPropertyName("max-udp-packet-size")] public string? MaxUdpPacketSize { get; set; }

        // ESTA É A PROPRIEDADE QUE ESTAVA EM FALTA:
        [JsonPropertyName("cache-size")] public string? CacheSize { get; set; }

        [JsonPropertyName("allow-remote-requests")] public string? AllowRemote { get; set; }
    }
}