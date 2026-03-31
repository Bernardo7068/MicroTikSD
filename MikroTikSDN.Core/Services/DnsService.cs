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

        // Único método necessário: aceita um objeto anónimo com qualquer campo
        // MikroTikSDN.Core/Services/DnsService.cs
        public async Task UpdateSettingsAsync(object dnsData)
        {
            // O URL tem de ser limpo. Se o teu RouterClient tiver lógica de ID, 
            // garante que aqui passas apenas o caminho base.
            await _client.PatchAsync("/rest/ip/dns", dnsData);
        }
    }
}