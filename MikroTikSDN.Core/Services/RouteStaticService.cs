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

        // CORRIGIDO: distance é opcional com valor default "1"
        public Task AddRouteAsync(string dstAddress, string gateway, string distance = "1")
            => _client.PutAsync("/rest/ip/route", new Dictionary<string, string>
            {
                ["dst-address"] = dstAddress,
                ["gateway"] = gateway,
                ["distance"] = distance
            });

        public Task DeleteRouteAsync(string id)
            => _client.DeleteAsync($"/rest/ip/route/{id}");
    }
}