using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MikroTikSDN.Core.Models
{
    public class BridgePortModel
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("bridge")] public string? Bridge { get; set; }
        [JsonPropertyName("interface")] public string? Interface { get; set; }
        [JsonPropertyName("disabled")] public bool Disabled { get; set; }
    }
}
