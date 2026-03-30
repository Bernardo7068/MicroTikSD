using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class DnsService
    {
        private readonly RouterClient _client;
        public DnsService(RouterClient client) => _client = client;

        public async Task<DnsSettingsModel> GetSettingsAsync() =>
            await _client.GetAsync<DnsSettingsModel>("/rest/ip/dns");

        public async Task UpdateSettingsAsync(string servers, bool allowRemote) =>
            await _client.PatchAsync("/rest/ip/dns", new { servers, @allow_remote_requests = allowRemote });
    }
}