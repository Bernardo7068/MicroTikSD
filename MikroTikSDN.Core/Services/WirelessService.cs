using System.Collections.Generic;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class WirelessService
    {
        private readonly RouterClient _client;
        public WirelessService(RouterClient client) => _client = client;

        // Interfaces Wireless
        public async Task<List<WirelessInterface>> GetWirelessAsync() =>
            await _client.GetAsync<List<WirelessInterface>>("/rest/interface/wireless");

    //    public async Task SetStateAsync(string id, bool enabled) =>
     //       await _client.PatchAsync($"/rest/interface/wireless/{id}", new { disabled = !enabled });

        // Perfis de Segurança
        public async Task<List<SecurityProfile>> GetProfilesAsync() =>
            await _client.GetAsync<List<SecurityProfile>>("/rest/interface/wireless/security-profiles");

        public async Task AddProfileAsync(string name, string authTypes) =>
            await _client.PutAsync("/rest/interface/wireless/security-profiles", new { name, @authentication_types = authTypes });

        public async Task UpdateWirelessSettingsAsync(string id, string ssid, string securityProfile) =>
    await _client.PatchAsync($"/rest/interface/wireless/{id}", new { ssid, @security_profile = securityProfile });

      

        public async Task UpdateInterfaceAsync(string id, object data) =>
    await _client.PatchAsync($"/rest/interface/wireless/{id}", data);


        // Adicionar Interface Virtual (VAP)
        public async Task AddVirtualInterfaceAsync(string master, string ssid)
        {
            // O MikroTik exige o master-interface e o modo ap-bridge para VAPs
            var data = new Dictionary<string, object>
    {
        { "master-interface", master },
        { "ssid", ssid },
        { "mode", "ap-bridge" },
        { "disabled", "no" }
    };
            await _client.PutAsync("/rest/interface/wireless", data);
        }

        // Adicionar Perfil de Segurança
        public async Task AddProfileAsync(string name, string authTypes, string ciphers, string password)
        {
            // IMPORTANTE: O 'mode' tem de ser 'dynamic-keys' para o router aceitar as chaves WPA/WPA2
            var data = new Dictionary<string, object>
    {
        { "name", name },
        { "mode", "dynamic-keys" },
        { "authentication-types", authTypes.Replace(" ", "") }, // Remove espaços
        { "unicast-ciphers", ciphers.Replace(" ", "") },       // Remove espaços
        { "group-ciphers", ciphers.Replace(" ", "") },         // Remove espaços
        { "wpa-pre-shared-key", password },
        { "wpa2-pre-shared-key", password }
    };
            await _client.PutAsync("/rest/interface/wireless/security-profiles", data);
        }

        // No MikroTikSDN.Core/Services/WirelessService.cs

        public async Task AddVirtualInterfaceAsync(string master, string ssid, string securityProfile)
        {
            var data = new Dictionary<string, object>
    {
        { "master-interface", master },
        { "ssid", ssid },
        { "security-profile", securityProfile },
        { "mode", "ap-bridge" },
        { "disabled", "no" }
    };

            // PUT para criar o recurso no MikroTik
            await _client.PutAsync("/rest/interface/wireless", data);
        }

        public async Task SetStateAsync(string id, bool disabled) =>
    await _client.PatchAsync($"/rest/interface/wireless/{id}", new { disabled = disabled.ToString().ToLower() });

        public async Task DeleteProfileAsync(string id) =>
            await _client.DeleteAsync($"/rest/interface/wireless/security-profiles/{id}");


        public async Task DeleteVirtualInterfaceAsync(string id) =>
            await _client.DeleteAsync($"/rest/interface/wireless/{id}");

        public async Task UpdateProfileAsync(string id, string name, string auth, string cipher, string psk) =>
    await _client.PatchAsync($"/rest/interface/wireless/security-profiles/{id}", new
    {
        name,
        @authentication_types = auth,
        @unicast_ciphers = cipher,
        @wpa2_pre_shared_key = psk
    });
    }
}