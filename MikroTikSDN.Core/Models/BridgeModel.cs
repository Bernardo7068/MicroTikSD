using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
namespace MikroTikSDN.Core.Models
{
    public class BridgeModel
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("comment")] public string? Comment { get; set; }
        [JsonPropertyName("disabled")] public string? Disabled { get; set; }
        [JsonPropertyName("running")] public string? Running { get; set; }
    }
}