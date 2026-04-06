using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MikroTikSDN.Core.Models;
using Org.BouncyCastle.Math.EC.Rfc7748; // Requer o NuGet: BouncyCastle.Cryptography

namespace MikroTikSDN.Core.Services
{
    public class WireGuardService
    {
        private readonly RouterClient _client;
        public WireGuardService(RouterClient client) => _client = client;

        // ─── Servidor (Interface WireGuard) ───────────────────────────────────

        public Task<List<WireGuardInterfaceModel>> GetInterfacesAsync()
            => _client.GetAsync<List<WireGuardInterfaceModel>>("/rest/interface/wireguard");

        public Task AddInterfaceAsync(string name, string listenPort)
            => _client.PutAsync("/rest/interface/wireguard", new Dictionary<string, string>
            {
                ["name"] = name,
                ["listen-port"] = listenPort
            });

        public Task DeleteInterfaceAsync(string id)
            => _client.DeleteAsync($"/rest/interface/wireguard/{id}");

        public Task SetInterfaceStateAsync(string id, bool disabled)
            => _client.PatchAsync($"/rest/interface/wireguard/{id}", new Dictionary<string, string>
            {
                ["disabled"] = disabled ? "yes" : "no"
            });

        public Task UpdateInterfaceAsync(string id, string name, string listenPort)
            => _client.PatchAsync($"/rest/interface/wireguard/{id}", new Dictionary<string, string>
            {
                ["name"] = name,
                ["listen-port"] = listenPort
            });

        // ─── Peers (Clientes VPN) ─────────────────────────────────────────────

        public Task<List<WireGuardPeerModel>> GetPeersAsync()
            => _client.GetAsync<List<WireGuardPeerModel>>("/rest/interface/wireguard/peers");

        public Task AddPeerAsync(string interfaceName, string clientPublicKey, string allowedIp, string clientName)
            => _client.PutAsync("/rest/interface/wireguard/peers", new Dictionary<string, string>
            {
                ["interface"] = interfaceName,
                ["public-key"] = clientPublicKey,
                ["allowed-address"] = allowedIp,
                ["comment"] = clientName
            });

        public Task DeletePeerAsync(string id)
            => _client.DeleteAsync($"/rest/interface/wireguard/peers/{id}");

        public Task SetPeerStateAsync(string id, bool disabled)
            => _client.PatchAsync($"/rest/interface/wireguard/peers/{id}", new Dictionary<string, string>
            {
                ["disabled"] = disabled ? "yes" : "no"
            });

        public Task UpdatePeerAsync(string id, string interfaceName, string publicKey, string allowedAddress, string comment)
            => _client.PatchAsync($"/rest/interface/wireguard/peers/{id}", new Dictionary<string, string>
            {
                ["interface"] = interfaceName,
                ["public-key"] = publicKey,
                ["allowed-address"] = allowedAddress,
                ["comment"] = comment
            });

        // ─── Ferramentas Avançadas (Chaves, Config e DNS) ──────────────────────

        public static string GenerateClientConfig(string privKey, string clientIp, string serverPubKey, string endpoint)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[Interface]");
            sb.AppendLine($"PrivateKey = {privKey.Trim()}");
            sb.AppendLine($"Address = {clientIp.Trim()}");
            sb.AppendLine("DNS = 8.8.8.8");
            sb.AppendLine();
            sb.AppendLine("[Peer]");
            sb.AppendLine($"PublicKey = {serverPubKey.Trim()}");
            sb.AppendLine($"Endpoint = {endpoint.Trim()}");
            sb.AppendLine("AllowedIPs = 0.0.0.0/0");
            sb.AppendLine("PersistentKeepalive = 25");
            return sb.ToString();
        }

        public static (string priv, string pub) GenerateKeyPair()
        {
            // 1. Gerar Privada
            byte[] privateKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(privateKey);
            }

            // 2. Clamping obrigatório do WireGuard
            privateKey[0] &= 248;
            privateKey[31] &= 127;
            privateKey[31] |= 64;

            // 3. Calcular Pública a partir da Privada (Curve25519 real)
            byte[] publicKey = new byte[32];
            X25519.ScalarMultBase(privateKey, 0, publicKey, 0);

            return (Convert.ToBase64String(privateKey), Convert.ToBase64String(publicKey));
        }

        public async Task<string> GetPublicEndpointAsync()
{
    // 1. TENTA O MIKROTIK (Se o DDNS estiver ligado)
    try
    {
        var cloud = await _client.GetAsync<MikroTikCloudModel>("/rest/ip/cloud");
        if (!string.IsNullOrEmpty(cloud?.DnsName)) return cloud.DnsName;
        if (!string.IsNullOrEmpty(cloud?.PublicAddress)) return cloud.PublicAddress;
    }
    catch { /* Ignora se o MikroTik der erro */ }

    // 2. TENTA A INTERNET (O atalho que não falha)
    try
    {
        using var http = new System.Net.Http.HttpClient();
        return (await http.GetStringAsync("https://api.ipify.org")).Trim();
    }
    catch { return ""; } // Falha total: O MainForm usará o IP local
}
    }
}