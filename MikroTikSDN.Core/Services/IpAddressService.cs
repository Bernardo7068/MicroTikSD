using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class IpAddressService
    {
        private readonly RouterClient _client;
        public IpAddressService(RouterClient client) => _client = client;

        public Task<List<IpAddressModel>> GetAddressesAsync()
            => _client.GetAsync<List<IpAddressModel>>("/rest/ip/address");

        public Task AddAddressAsync(string address, string iface)
            => _client.PutAsync("/rest/ip/address", new Dictionary<string, string>
            {
                ["address"] = address,
                ["interface"] = iface
            });

        public Task UpdateAddressAsync(string id, string address, string iface)
            => _client.PatchAsync($"/rest/ip/address/{id}", new Dictionary<string, string>
            {
                ["address"] = address,
                ["interface"] = iface
            });

        public Task DeleteAddressAsync(string id)
            => _client.DeleteAsync($"/rest/ip/address/{id}");

        public Task SetStateAsync(string id, bool disabled) =>
    _client.PatchAsync($"/rest/ip/address/{id}", new Dictionary<string, string> { ["disabled"] = disabled ? "yes" : "no" });
    }
}