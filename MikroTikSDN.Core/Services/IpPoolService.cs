using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class IpPoolService
    {
        private readonly RouterClient _client;
        public IpPoolService(RouterClient client) => _client = client;

        // Lista todas as Pools (IP -> Pool no Winbox)
        public async Task<List<IpPoolModel>> GetAllAsync() =>
            await _client.GetListAsync<IpPoolModel>("/rest/ip/pool");

        // Atualiza o nome ou o intervalo de IPs
        public async Task UpdateAsync(string id, string name, string ranges) =>
            await _client.PatchAsync($"/rest/ip/pool/{id}", new { name, ranges });
    }
}