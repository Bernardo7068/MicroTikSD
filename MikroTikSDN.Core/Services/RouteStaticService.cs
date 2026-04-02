using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class RouteService
    {
        private readonly RouterClient _client;
        public RouteService(RouterClient client) => _client = client;

        public Task<List<StaticRouteModel>> GetRoutesAsync()
            => _client.GetAsync<List<StaticRouteModel>>("/rest/ip/route");

        public Task AddRouteAsync(string dstAddress, string gateway, string distance = "1")
            => _client.PutAsync("/rest/ip/route", new Dictionary<string, string>
            {
                ["dst-address"] = dstAddress,   // hífen correto
                ["gateway"] = gateway,
                ["distance"] = distance
            });

        // CORRIGIDO: @dst_address → "dst-address" via Dictionary
        public Task UpdateRouteAsync(string id, string dst, string gateway, string distance, string comment)
            => _client.PatchAsync($"/rest/ip/route/{id}", new Dictionary<string, string>
            {
                ["dst-address"] = dst,
                ["gateway"] = gateway,
                ["distance"] = distance,
                ["comment"] = comment
            });

        public Task DeleteRouteAsync(string id)
            => _client.DeleteAsync($"/rest/ip/route/{id}");

        public Task SetStateAsync(string id, bool disabled) =>
    _client.PatchAsync($"/rest/ip/route/{id}", new Dictionary<string, string> { ["disabled"] = disabled ? "yes" : "no" });
    }
}