using System.Collections.Generic;
using System.Linq; // Adicionado para o filtro .Where funcionar
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
        /// Atualiza definições DNS. 
        /// Usa POST em /set para evitar erro 400 (missing resource identifier).
        /// </summary>
        public Task UpdateSettingsAsync(Dictionary<string, string> data)
        {
            // Sem filtros! Envia o dicionário todo direto para o /set
            // Assim o "" chega ao MikroTik e ele percebe que é para apagar.
            return _client.PostAsync("/rest/ip/dns/set", data);
        }

        // Atalho conveniente para o caso simples (servers + allow-remote)
        public Task UpdateSettingsAsync(string servers, bool allowRemote)
            => UpdateSettingsAsync(new Dictionary<string, string>
            {
                ["servers"] = servers,
                ["allow-remote-requests"] = allowRemote ? "yes" : "no"
            });
    }
}