using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class IpPoolService
    {
        private readonly RouterClient _client;
        public IpPoolService(RouterClient client) => _client = client;

        public Task<List<IpPoolModel>> GetAllAsync()
            => _client.GetAsync<List<IpPoolModel>>("/rest/ip/pool");

        public Task AddAsync(string name, string ranges)
            => _client.PutAsync("/rest/ip/pool", new Dictionary<string, string>
            {
                ["name"] = name,
                ["ranges"] = ranges
            });

        public Task UpdateAsync(string id, string name, string ranges)
            => _client.PatchAsync($"/rest/ip/pool/{id}", new Dictionary<string, string>
            {
                ["name"] = name,
                ["ranges"] = ranges
            });

        public Task DeleteAsync(string id)
            => _client.DeleteAsync($"/rest/ip/pool/{id}");
    }
}