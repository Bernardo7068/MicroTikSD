using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MikroTikSDN.Core.Models
{
    public class DnsSettingsModel
    {
        [JsonPropertyName("servers")] public string? Servers { get; set; }
        [JsonPropertyName("allow-remote-requests")] public bool AllowRemote { get; set; }
        [JsonPropertyName("cache-size")] public string? CacheSize { get; set; }
    }
}