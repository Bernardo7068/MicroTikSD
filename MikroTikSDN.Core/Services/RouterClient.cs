using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MikroTikSDN.Core.Exceptions;
using MikroTikSDN.Core.Models;

namespace MikroTikSDN.Core.Services
{
    public class RouterClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        // Opções de desserialização: aceita tanto "Name" como "name" da API
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public RouterClient(RouterDevice device)
        {
            var handler = new HttpClientHandler
            {
                // Ignora erros de certificado SSL (comum em routers MikroTik)
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri($"https://{device.IpAddress}"),
                Timeout = TimeSpan.FromSeconds(15)
            };

            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{device.Username}:{device.Password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ─── Métodos base ─────────────────────────────────────────────────────

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            await EnsureSuccessAsync(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
        }

        // PUT = criar novo recurso na API do MikroTik
        public async Task PutAsync(string endpoint, Dictionary<string, string> data)
        {
            var content = CreateJsonContent(data);
            var response = await _httpClient.PutAsync(endpoint, content);
            await EnsureSuccessAsync(response);
        }

        // PATCH = editar recurso existente
        public async Task PatchAsync(string endpoint, Dictionary<string, string> data)
        {
            var content = CreateJsonContent(data);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint) { Content = content };
            var response = await _httpClient.SendAsync(request);
            await EnsureSuccessAsync(response);
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            await EnsureSuccessAsync(response);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/rest/system/resource");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static StringContent CreateJsonContent(Dictionary<string, string> data)
        {
            // Usar Dictionary garante que as chaves com hífens (ex: "dst-address") ficam corretas no JSON
            var json = JsonSerializer.Serialize(data);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            var body = await response.Content.ReadAsStringAsync();
            var msg = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Credenciais inválidas.",
                HttpStatusCode.Forbidden => "Sem permissões para esta operação.",
                HttpStatusCode.NotFound => "Recurso não encontrado.",
                HttpStatusCode.BadRequest => $"Pedido inválido: {body}",
                _ => $"Erro {(int)response.StatusCode}: {body}"
            };

            throw new RouterApiException(msg, response.StatusCode, body);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}