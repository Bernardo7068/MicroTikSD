using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class DhcpService
    {
        private readonly RouterClient _client;
        public DhcpService(RouterClient client) => _client = client;

        // --- DHCP SERVER (Usa o teu DhcpModel) ---
        public async Task<List<DhcpModel>> GetServersAsync() =>
            await _client.GetListAsync<DhcpModel>("/rest/ip/dhcp-server");

        public async Task AddServerAsync(string name, string iface, string pool)
        {
            // Usamos um Dictionary para poder escrever o nome com HÍFEN
            var data = new Dictionary<string, object>
    {
        { "name", name },
        { "interface", iface },
        { "address-pool", pool } // O MikroTik exige o hífen aqui!
    };

            await _client.PutAsync("/rest/ip/dhcp-server", data);
        }

        public async Task UpdateServerAsync(string id, string name, string iface, string pool)
        {
            var data = new Dictionary<string, object>
    {
        { "name", name },
        { "interface", iface },
        { "address-pool", pool }
    };

            await _client.PatchAsync($"/rest/ip/dhcp-server/{id}", data);
        }

        public async Task DeleteServerAsync(string id) =>
            await _client.DeleteAsync($"/rest/ip/dhcp-server/{id}");


        // --- DHCP CLIENT ---
        public async Task<List<DhcpClientModel>> GetClientsAsync() =>
            await _client.GetListAsync<DhcpClientModel>("/rest/ip/dhcp-client");

        public async Task AddClientAsync(string iface, bool useDns, bool addRoute)
        {
            var data = new Dictionary<string, object>
    {
        { "interface", iface },
        { "use-peer-dns", useDns ? "yes" : "no" },
        { "add-default-route", addRoute ? "yes" : "no" }
    };

            await _client.PutAsync("/rest/ip/dhcp-client", data);
        }

        public async Task UpdateClientAsync(string id, string iface, bool useDns, bool addRoute) =>
            await _client.PatchAsync($"/rest/ip/dhcp-client/{id}", new
            {
                @interface = iface,
                @use_peer_dns = useDns ? "yes" : "no",
                @add_default_route = addRoute ? "yes" : "no"
            });

        public async Task DeleteClientAsync(string id) =>
            await _client.DeleteAsync($"/rest/ip/dhcp-client/{id}");
    }
}