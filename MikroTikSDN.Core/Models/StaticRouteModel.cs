using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
namespace MikroTikSDN.Core.Models
{
    public class StaticRouteModel
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("dst-address")] public string? DstAddress { get; set; }
        [JsonPropertyName("gateway")] public string? Gateway { get; set; }
        // NOTA: Distance vem como string da API do RouterOS — não usar int
        [JsonPropertyName("distance")] public string? Distance { get; set; }
        //[JsonPropertyName("active")] public string? Active { get; set; }
        [JsonPropertyName("disabled")] public string? Disabled { get; set; }
        [JsonPropertyName("comment")] public string? Comment { get; set; }
    }
}