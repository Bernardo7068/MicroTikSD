using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class WirelessService
    {
        private readonly RouterClient _client;
        public WirelessService(RouterClient client) => _client = client;

        // Interfaces Wireless
        public async Task<List<WirelessInterface>> GetWirelessAsync() =>
            await _client.GetAsync<List<WirelessInterface>>("/rest/interface/wireless");

        public async Task SetStateAsync(string id, bool enabled) =>
            await _client.PatchAsync($"/rest/interface/wireless/{id}", new { disabled = !enabled });

        // Perfis de Segurança
        public async Task<List<SecurityProfile>> GetProfilesAsync() =>
            await _client.GetAsync<List<SecurityProfile>>("/rest/interface/wireless/security-profiles");

        public async Task AddProfileAsync(string name, string authTypes) =>
            await _client.PutAsync("/rest/interface/wireless/security-profiles", new { name, @authentication_types = authTypes });

        public async Task DeleteProfileAsync(string id) =>
            await _client.DeleteAsync($"/rest/interface/wireless/security-profiles/{id}");
    }
}