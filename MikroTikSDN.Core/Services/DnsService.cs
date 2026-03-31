using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class DnsService
    {
        private readonly RouterClient _client;
        public DnsService(RouterClient client) => _client = client;

        public Task<DnsSettingsModel> GetSettingsAsync()
            => _client.GetAsync<DnsSettingsModel>("/rest/ip/dns");

        /// <summary>
        /// Atualiza definições DNS. Aceita qualquer subconjunto de campos.
        /// Usar sempre Dictionary com hífens (ex: "allow-remote-requests").
        /// </summary>
        public Task UpdateSettingsAsync(Dictionary<string, string> data)
            => _client.PatchAsync("/rest/ip/dns", data);

        // Atalho conveniente para o caso simples (servers + allow-remote)
        public Task UpdateSettingsAsync(string servers, bool allowRemote)
            => UpdateSettingsAsync(new Dictionary<string, string>
            {
                ["servers"] = servers,
                ["allow-remote-requests"] = allowRemote ? "yes" : "no"
            });
    }
}