using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class BridgeService
    {
        private readonly RouterClient _client;
        public BridgeService(RouterClient client) => _client = client;

        // ─── Bridges ──────────────────────────────────────────────────────────

        public Task<List<BridgeModel>> GetBridgesAsync()
            => _client.GetAsync<List<BridgeModel>>("/rest/interface/bridge");

        public Task AddBridgeAsync(string name, string protocolMode = "rstp", bool vlanFiltering = false)
            => _client.PutAsync("/rest/interface/bridge", new Dictionary<string, string>
            {
                ["name"] = name,
                ["protocol-mode"] = protocolMode,           // hífen correto
                ["vlan-filtering"] = vlanFiltering ? "yes" : "no"
            });

        // CORRIGIDO: @protocol_mode → "protocol-mode" via Dictionary
        public Task UpdateBridgeAsync(string id, string name, string protocolMode)
            => _client.PatchAsync($"/rest/interface/bridge/{id}", new Dictionary<string, string>
            {
                ["name"] = name,
                ["protocol-mode"] = protocolMode             // hífen correto
            });

        public Task DeleteBridgeAsync(string id)
            => _client.DeleteAsync($"/rest/interface/bridge/{id}");

        // ─── Bridge Ports ─────────────────────────────────────────────────────

        public Task<List<BridgePortModel>> GetPortsAsync()
            => _client.GetAsync<List<BridgePortModel>>("/rest/interface/bridge/port");

        public Task AddPortToBridgeAsync(string bridge, string iface, string pvid = "1")
            => _client.PutAsync("/rest/interface/bridge/port", new Dictionary<string, string>
            {
                ["bridge"] = bridge,
                ["interface"] = iface,
                ["pvid"] = pvid
            });

        public Task UpdatePortAsync(string id, string bridge, string iface, string pvid)
            => _client.PatchAsync($"/rest/interface/bridge/port/{id}", new Dictionary<string, string>
            {
                ["bridge"] = bridge,
                ["interface"] = iface,
                ["pvid"] = pvid
            });

        public Task DeleteBridgePortAsync(string id)
            => _client.DeleteAsync($"/rest/interface/bridge/port/{id}");

        public Task SetBridgeStateAsync(string id, bool disabled) =>
    _client.PatchAsync($"/rest/interface/bridge/{id}", new Dictionary<string, string> { ["disabled"] = disabled ? "yes" : "no" });

        public Task SetPortStateAsync(string id, bool disabled) =>
            _client.PatchAsync($"/rest/interface/bridge/port/{id}", new Dictionary<string, string> { ["disabled"] = disabled ? "yes" : "no" });
    }
}