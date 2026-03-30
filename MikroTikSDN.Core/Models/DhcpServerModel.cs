using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
namespace MikroTikSDN.Core.Models
{
    public class DhcpModel
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("interface")] public string? Interface { get; set; }
        [JsonPropertyName("address-pool")] public string? AddressPool { get; set; }
        [JsonPropertyName("disabled")] public string? Disabled { get; set; }
        [JsonPropertyName("lease-time")] public string? LeaseTime { get; set; }
    }
}