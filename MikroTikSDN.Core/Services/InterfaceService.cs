using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class InterfaceService
    {
        private readonly RouterClient _client;

        public InterfaceService(RouterClient client) => _client = client;

        public async Task<List<NetworkInterface>> GetAllAsync() =>
            await _client.GetAsync<List<NetworkInterface>>("/rest/interface");

        // ADICIONA ESTE MÉTODO PARA PODERES APAGAR AS VIRTUAIS:
        public async Task DeleteInterfaceAsync(string id) =>
            await _client.DeleteAsync($"/rest/interface/{id}");

        public Task SetStateAsync(string id, bool disabled) =>
    _client.PatchAsync($"/rest/interface/{id}", new Dictionary<string, string> { ["disabled"] = disabled ? "yes" : "no" });
    }
}