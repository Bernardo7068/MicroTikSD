using System.Text.Json.Serialization;

namespace MikroTikSDN.Core.Models
{
    public class NetworkInterface
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("mtu")] public string? Mtu { get; set; }
        [JsonPropertyName("mac-address")] public string? MacAddress { get; set; }
        [JsonPropertyName("running")] public bool Running { get; set; }
        [JsonPropertyName("disabled")] public bool Disabled { get; set; }
        [JsonPropertyName("comment")] public string? Comment { get; set; }
    }

    public class WirelessInterface
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("ssid")] public string? Ssid { get; set; }
        [JsonPropertyName("mode")] public string? Mode { get; set; }
        [JsonPropertyName("band")] public string? Band { get; set; }
        [JsonPropertyName("channel-width")] public string? ChannelWidth { get; set; }
        [JsonPropertyName("frequency")] public string? Frequency { get; set; }
        [JsonPropertyName("security-profile")] public string? SecurityProfile { get; set; }
        [JsonPropertyName("disabled")] public bool Disabled { get; set; }
        [JsonPropertyName("running")] public bool Running { get; set; }
    }

    public class SecurityProfile
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("mode")] public string? Mode { get; set; }
        [JsonPropertyName("authentication-types")] public string? AuthenticationTypes { get; set; }
        [JsonPropertyName("wpa2-pre-shared-key")] public string? Wpa2PreSharedKey { get; set; }
        [JsonPropertyName("wpa-pre-shared-key")] public string? WpaPreSharedKey { get; set; }
    }

    public class Bridge
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("mtu")] public string? Mtu { get; set; }
        [JsonPropertyName("protocol-mode")] public string? ProtocolMode { get; set; }
        [JsonPropertyName("disabled")] public bool Disabled { get; set; }
        [JsonPropertyName("running")] public bool Running { get; set; }
        [JsonPropertyName("comment")] public string? Comment { get; set; }
    }

    public class BridgePort
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("bridge")] public string? Bridge { get; set; }
        [JsonPropertyName("interface")] public string? Interface { get; set; }
        [JsonPropertyName("horizon")] public string? Horizon { get; set; }
        [JsonPropertyName("disabled")] public bool Disabled { get; set; }
    }

    public class IpAddress
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("address")] public string? Address { get; set; }
        [JsonPropertyName("network")] public string? Network { get; set; }
        [JsonPropertyName("interface")] public string? Interface { get; set; }
        [JsonPropertyName("disabled")] public bool Disabled { get; set; }
        [JsonPropertyName("comment")] public string? Comment { get; set; }
    }

    public class StaticRoute
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("dst-address")] public string? DstAddress { get; set; }
        [JsonPropertyName("gateway")] public string? Gateway { get; set; }
        [JsonPropertyName("distance")] public string? Distance { get; set; }
        [JsonPropertyName("interface")] public string? Interface { get; set; }
        [JsonPropertyName("active")] public bool Active { get; set; }
        [JsonPropertyName("disabled")] public bool Disabled { get; set; }
        [JsonPropertyName("comment")] public string? Comment { get; set; }
    }

    public class DhcpServer
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("interface")] public string? Interface { get; set; }
        [JsonPropertyName("address-pool")] public string? AddressPool { get; set; }
        [JsonPropertyName("lease-time")] public string? LeaseTime { get; set; }
        [JsonPropertyName("disabled")] public bool Disabled { get; set; }
    }

    public class DhcpPool
    {
        [JsonPropertyName(".id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("ranges")] public string? Ranges { get; set; }
    }

    public class DnsSettings
    {
        [JsonPropertyName("servers")] public string? Servers { get; set; }
        [JsonPropertyName("allow-remote-requests")] public bool AllowRemoteRequests { get; set; }
        [JsonPropertyName("cache-size")] public string? CacheSize { get; set; }
        [JsonPropertyName("cache-max-ttl")] public string? CacheMaxTtl { get; set; }
    }

    public class SystemIdentity
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
    }

    public class SystemResource
    {
        [JsonPropertyName("uptime")] public string? Uptime { get; set; }
        [JsonPropertyName("version")] public string? Version { get; set; }
        [JsonPropertyName("cpu-load")] public string? CpuLoad { get; set; }
        [JsonPropertyName("free-memory")] public string? FreeMemory { get; set; }
        [JsonPropertyName("total-memory")] public string? TotalMemory { get; set; }
        [JsonPropertyName("board-name")] public string? BoardName { get; set; }
        [JsonPropertyName("architecture-name")] public string? Architecture { get; set; }
    }
}