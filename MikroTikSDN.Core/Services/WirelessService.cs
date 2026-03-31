using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class WirelessService
    {
        private readonly RouterClient _client;
        public WirelessService(RouterClient client) => _client = client;

        // ─── Interfaces Wireless ──────────────────────────────────────────────

        public Task<List<WirelessInterface>> GetWirelessAsync()
            => _client.GetAsync<List<WirelessInterface>>("/rest/interface/wireless");

        public Task SetStateAsync(string id, bool disabled)
            => _client.PatchAsync($"/rest/interface/wireless/{id}", new Dictionary<string, string>
            {
                ["disabled"] = disabled ? "yes" : "no"
            });

        // CORRIGIDO: @security_profile → "security-profile" via Dictionary
        public Task UpdateWirelessSettingsAsync(string id, string ssid, string securityProfile)
            => _client.PatchAsync($"/rest/interface/wireless/{id}", new Dictionary<string, string>
            {
                ["ssid"] = ssid,
                ["security-profile"] = securityProfile   // hífen correto
            });

        public Task UpdateInterfaceAsync(string id, Dictionary<string, string> data)
            => _client.PatchAsync($"/rest/interface/wireless/{id}", data);

        // Interface Virtual (VAP) — com perfil de segurança
        public Task AddVirtualInterfaceAsync(string master, string ssid, string securityProfile = "default")
            => _client.PutAsync("/rest/interface/wireless", new Dictionary<string, string>
            {
                ["master-interface"] = master,            // hífen correto
                ["ssid"] = ssid,
                ["security-profile"] = securityProfile,  // hífen correto
                ["mode"] = "ap-bridge",
                ["disabled"] = "no"
            });

        public Task DeleteVirtualInterfaceAsync(string id)
            => _client.DeleteAsync($"/rest/interface/wireless/{id}");

        // ─── Perfis de Segurança ──────────────────────────────────────────────

        public Task<List<SecurityProfile>> GetProfilesAsync()
            => _client.GetAsync<List<SecurityProfile>>("/rest/interface/wireless/security-profiles");

        // CORRIGIDO: @authentication_types → "authentication-types" via Dictionary
        public Task AddProfileAsync(string name, string authTypes, string ciphers, string password)
            => _client.PutAsync("/rest/interface/wireless/security-profiles", new Dictionary<string, string>
            {
                ["name"] = name,
                ["mode"] = "dynamic-keys",
                ["authentication-types"] = authTypes.Replace(" ", ""),  // hífen correto
                ["unicast-ciphers"] = ciphers.Replace(" ", ""),    // hífen correto
                ["group-ciphers"] = ciphers.Replace(" ", ""),    // hífen correto
                ["wpa-pre-shared-key"] = password,                    // hífen correto
                ["wpa2-pre-shared-key"] = password                     // hífen correto
            });

        // CORRIGIDO: @authentication_types, @unicast_ciphers, @wpa2_pre_shared_key → Dictionary
        public Task UpdateProfileAsync(string id, string name, string auth, string cipher, string psk)
            => _client.PatchAsync($"/rest/interface/wireless/security-profiles/{id}", new Dictionary<string, string>
            {
                ["name"] = name,
                ["authentication-types"] = auth,    // hífen correto
                ["unicast-ciphers"] = cipher,  // hífen correto
                ["wpa2-pre-shared-key"] = psk      // hífen correto
            });

        public Task DeleteProfileAsync(string id)
            => _client.DeleteAsync($"/rest/interface/wireless/security-profiles/{id}");
    }
}