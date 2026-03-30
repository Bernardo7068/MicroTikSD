using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class IpAddressService
    {
        private readonly RouterClient _client;
        public IpAddressService(RouterClient client) => _client = client;

        public async Task<List<IpAddressModel>> GetAddressesAsync() =>
            await _client.GetAsync<List<IpAddressModel>>("/rest/ip/address");

        public async Task AddAddressAsync(string address, string @interface) =>
            await _client.PutAsync("/rest/ip/address", new { address, @interface });

        public async Task UpdateAddressAsync(string id, string address) =>
            await _client.PatchAsync($"/rest/ip/address/{id}", new { address });

        public async Task DeleteAddressAsync(string id) =>
            await _client.DeleteAsync($"/rest/ip/address/{id}");
    }
}