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
    }
}