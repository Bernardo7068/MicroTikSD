using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class InterfaceService
    {
        private readonly RouterClient _client;
        public InterfaceService(RouterClient client) => _client = client;

        public Task<List<NetworkInterface>> GetAllAsync() => _client.GetAsync<List<NetworkInterface>>("interface");
        public Task<List<WirelessInterface>> GetWirelessAsync() => _client.GetAsync<List<WirelessInterface>>("interface/wireless");

        public Task EnableWirelessAsync(string id)
            => _client.PostAsync("interface/wireless/enable",
                new Dictionary<string, string> { [".id"] = id });

        public Task DisableWirelessAsync(string id)
            => _client.PostAsync("interface/wireless/disable",
                new Dictionary<string, string> { [".id"] = id });

        public Task<WirelessInterface?> UpdateWirelessAsync(string id, object changes)
            => _client.PatchAsync<WirelessInterface>("interface/wireless", id, changes);
    }
}