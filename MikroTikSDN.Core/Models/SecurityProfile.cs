using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
namespace MikroTikSDN.Core.Models
{
    public class SecurityProfile
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("authentication-types")] public string? AuthTypes { get; set; }
        [JsonPropertyName("mode")] public string? Mode { get; set; }
    }
}