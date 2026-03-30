using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class DhcpService
    {
        private readonly RouterClient _client;
        public DhcpService(RouterClient client) => _client = client;

        public async Task<List<DhcpModel>> GetServersAsync() =>
            await _client.GetAsync<List<DhcpModel>>("/rest/ip/dhcp-server");

        public async Task AddServerAsync(string name, string @interface, string pool) =>
            await _client.PutAsync("/rest/ip/dhcp-server", new { name, @interface, @address_pool = pool });

        public async Task DeleteServerAsync(string id) =>
            await _client.DeleteAsync($"/rest/ip/dhcp-server/{id}");
    }
}