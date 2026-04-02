using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class DhcpService
    {
        private readonly RouterClient _client;
        public DhcpService(RouterClient client) => _client = client;

        // ─── DHCP Server ──────────────────────────────────────────────────────

        public Task<List<DhcpModel>> GetServersAsync()
            => _client.GetAsync<List<DhcpModel>>("/rest/ip/dhcp-server");

        public Task AddServerAsync(string name, string iface, string pool)
            => _client.PutAsync("/rest/ip/dhcp-server", new Dictionary<string, string>
            {
                ["name"] = name,
                ["interface"] = iface,
                ["address-pool"] = pool,   // hífen correto
                ["disabled"] = "no"
            });

        public Task UpdateServerAsync(string id, string name, string iface, string pool)
            => _client.PatchAsync($"/rest/ip/dhcp-server/{id}", new Dictionary<string, string>
            {
                ["name"] = name,
                ["interface"] = iface,
                ["address-pool"] = pool   // hífen correto
            });

        public Task DeleteServerAsync(string id)
            => _client.DeleteAsync($"/rest/ip/dhcp-server/{id}");

        public Task SetServerStateAsync(string id, bool disabled) =>
    _client.PatchAsync($"/rest/ip/dhcp-server/{id}", new Dictionary<string, string> { ["disabled"] = disabled ? "yes" : "no" });

        // ─── DHCP Client ──────────────────────────────────────────────────────

        public Task<List<DhcpClientModel>> GetClientsAsync()
            => _client.GetAsync<List<DhcpClientModel>>("/rest/ip/dhcp-client");

        public Task AddClientAsync(string iface, bool useDns, bool addRoute)
            => _client.PutAsync("/rest/ip/dhcp-client", new Dictionary<string, string>
            {
                ["interface"] = iface,
                ["use-peer-dns"] = useDns ? "yes" : "no",  // hífen correto
                ["add-default-route"] = addRoute ? "yes" : "no"   // hífen correto
            });

        // CORRIGIDO: @use_peer_dns / @add_default_route → Dictionary com hífens
        public Task UpdateClientAsync(string id, string iface, bool useDns, bool addRoute)
            => _client.PatchAsync($"/rest/ip/dhcp-client/{id}", new Dictionary<string, string>
            {
                ["interface"] = iface,
                ["use-peer-dns"] = useDns ? "yes" : "no",
                ["add-default-route"] = addRoute ? "yes" : "no"
            });

        public Task DeleteClientAsync(string id)
            => _client.DeleteAsync($"/rest/ip/dhcp-client/{id}");


        public Task SetClientStateAsync(string id, bool disabled) =>
    _client.PatchAsync($"/rest/ip/dhcp-client/{id}", new Dictionary<string, string> { ["disabled"] = disabled ? "yes" : "no" });


        public Task AddNetworkAsync(string address, string gateway, string dnsServer)
    => _client.PutAsync("/rest/ip/dhcp-server/network", new Dictionary<string, string>
    {
        ["address"] = address,
        ["gateway"] = gateway,
        ["dns-server"] = dnsServer // O Winbox costuma sugerir o próprio Gateway ou o do ISP
    });
    }
}