using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class BridgeService
    {
        private readonly RouterClient _client;
        public BridgeService(RouterClient client) => _client = client;

        public Task<List<BridgeModel>> GetBridgesAsync()
            => _client.GetAsync<List<BridgeModel>>("/rest/interface/bridge");

        public Task<List<BridgePortModel>> GetPortsAsync()
            => _client.GetAsync<List<BridgePortModel>>("/rest/interface/bridge/port");

        public Task AddBridgeAsync(string name)
            => _client.PutAsync("/rest/interface/bridge", new Dictionary<string, string>
            {
                ["name"] = name
            });

        public Task AddPortToBridgeAsync(string bridge, string iface)
            => _client.PutAsync("/rest/interface/bridge/port", new Dictionary<string, string>
            {
                ["bridge"] = bridge,
                ["interface"] = iface
            });

        public Task DeleteBridgeAsync(string id)
            => _client.DeleteAsync($"/rest/interface/bridge/{id}");

        // CORRIGIDO: método em falta que o MainForm.cs precisa
        public Task DeleteBridgePortAsync(string id)
            => _client.DeleteAsync($"/rest/interface/bridge/port/{id}");
    }
}