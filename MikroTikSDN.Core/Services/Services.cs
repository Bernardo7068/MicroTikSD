using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class BridgeService
    {
        private readonly RouterClient _client;
        public BridgeService(RouterClient client) => _client = client;

        public Task<List<Bridge>> GetBridgesAsync() => _client.GetAsync<List<Bridge>>("interface/bridge");
        public Task<List<BridgePort>> GetPortsAsync() => _client.GetAsync<List<BridgePort>>("interface/bridge/port");

        public Task<Bridge?> CreateBridgeAsync(object data) => _client.PostAsync<Bridge>("interface/bridge", data);
        public Task<Bridge?> UpdateBridgeAsync(string id, object data) => _client.PatchAsync<Bridge>("interface/bridge", id, data);
        public Task DeleteBridgeAsync(string id) => _client.DeleteAsync("interface/bridge", id);

        public Task<BridgePort?> AddPortAsync(string bridgeName, string interfaceName)
            => _client.PostAsync<BridgePort>("interface/bridge/port",
                new Dictionary<string, string>
                {
                    ["bridge"] = bridgeName,
                    ["interface"] = interfaceName
                });

        public Task<BridgePort?> UpdatePortAsync(string id, object data) => _client.PatchAsync<BridgePort>("interface/bridge/port", id, data);
        public Task RemovePortAsync(string id) => _client.DeleteAsync("interface/bridge/port", id);
    }

    public class SecurityProfileService
    {
        private readonly RouterClient _client;
        public SecurityProfileService(RouterClient client) => _client = client;

        public Task<List<SecurityProfile>> GetAllAsync()
            => _client.GetAsync<List<SecurityProfile>>("interface/wireless/security-profiles");

        public Task<SecurityProfile?> CreateAsync(object data)
            => _client.PostAsync<SecurityProfile>("interface/wireless/security-profiles", data);

        public Task<SecurityProfile?> UpdateAsync(string id, object data)
            => _client.PatchAsync<SecurityProfile>("interface/wireless/security-profiles", id, data);

        public Task DeleteAsync(string id)
            => _client.DeleteAsync("interface/wireless/security-profiles", id);
    }

    public class IpAddressService
    {
        private readonly RouterClient _client;
        public IpAddressService(RouterClient client) => _client = client;

        public Task<List<IpAddress>> GetAllAsync()
            => _client.GetAsync<List<IpAddress>>("ip/address");

        public Task<IpAddress?> CreateAsync(string address, string iface)
            => _client.PostAsync<IpAddress>("ip/address",
                new Dictionary<string, string>
                {
                    ["address"] = address,
                    ["interface"] = iface
                });

        public Task<IpAddress?> UpdateAsync(string id, object data)
            => _client.PatchAsync<IpAddress>("ip/address", id, data);

        public Task DeleteAsync(string id)
            => _client.DeleteAsync("ip/address", id);
    }

    public class RouteService
    {
        private readonly RouterClient _client;
        public RouteService(RouterClient client) => _client = client;

        public Task<List<StaticRoute>> GetAllAsync()
            => _client.GetAsync<List<StaticRoute>>("ip/route");

        public Task<StaticRoute?> CreateAsync(string dstAddress, string gateway, string distance = "1")
            => _client.PostAsync<StaticRoute>("ip/route",
                new Dictionary<string, string>
                {
                    ["dst-address"] = dstAddress,
                    ["gateway"] = gateway,
                    ["distance"] = distance
                });

        public Task<StaticRoute?> UpdateAsync(string id, object data)
            => _client.PatchAsync<StaticRoute>("ip/route", id, data);

        public Task DeleteAsync(string id)
            => _client.DeleteAsync("ip/route", id);
    }

    public class DhcpService
    {
        private readonly RouterClient _client;
        public DhcpService(RouterClient client) => _client = client;

        public Task<List<DhcpServer>> GetServersAsync() => _client.GetAsync<List<DhcpServer>>("ip/dhcp-server");
        public Task<DhcpServer?> CreateServerAsync(object data) => _client.PostAsync<DhcpServer>("ip/dhcp-server", data);
        public Task<DhcpServer?> UpdateServerAsync(string id, object data) => _client.PatchAsync<DhcpServer>("ip/dhcp-server", id, data);
        public Task DeleteServerAsync(string id) => _client.DeleteAsync("ip/dhcp-server", id);

        public Task<List<DhcpPool>> GetPoolsAsync()
            => _client.GetAsync<List<DhcpPool>>("ip/pool");

        public Task<DhcpPool?> CreatePoolAsync(string name, string ranges)
            => _client.PostAsync<DhcpPool>("ip/pool",
                new Dictionary<string, string> { ["name"] = name, ["ranges"] = ranges });

        public Task<DhcpPool?> UpdatePoolAsync(string id, object data)
            => _client.PatchAsync<DhcpPool>("ip/pool", id, data);

        public Task DeletePoolAsync(string id)
            => _client.DeleteAsync("ip/pool", id);
    }

    public class DnsService
    {
        private readonly RouterClient _client;
        public DnsService(RouterClient client) => _client = client;

        public Task<DnsSettings> GetSettingsAsync()
            => _client.GetAsync<DnsSettings>("ip/dns");

        public Task SetSettingsAsync(string servers, bool allowRemoteRequests)
            => _client.PostAsync("ip/dns/set",
                new Dictionary<string, string>
                {
                    ["servers"] = servers,
                    ["allow-remote-requests"] = allowRemoteRequests ? "yes" : "no"
                });

        public Task EnableAsync() => SetRemoteRequestsAsync(true);
        public Task DisableAsync() => SetRemoteRequestsAsync(false);

        private Task SetRemoteRequestsAsync(bool enable)
            => _client.PostAsync("ip/dns/set",
                new Dictionary<string, string>
                {
                    ["allow-remote-requests"] = enable ? "yes" : "no"
                });
    }

    public class SystemService
    {
        private readonly RouterClient _client;
        public SystemService(RouterClient client) => _client = client;

        public Task<SystemIdentity> GetIdentityAsync() => _client.GetAsync<SystemIdentity>("system/identity");
        public Task<SystemResource> GetResourcesAsync() => _client.GetAsync<SystemResource>("system/resource");

        public Task SetIdentityAsync(string name)
            => _client.PostAsync("system/identity/set",
                new Dictionary<string, string> { ["name"] = name });
    }
}