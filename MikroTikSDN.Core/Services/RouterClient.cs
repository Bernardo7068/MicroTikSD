using MikroTikSDN.Core.Exceptions;
using MikroTikSDN.Core.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MikroTikSDN.Core.Services
{
    public class RouterClient : IDisposable

    {
        private readonly HttpClient _httpClient;


        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public RouterClient(RouterDevice device)
        {
            var handler = new HttpClientHandler
            {
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

        public async Task PostAsync(string endpoint, object data)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Usa PostAsync em vez de PatchAsync
            var response = await _httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new System.Exception(error);
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            await EnsureSuccessAsync(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
        }

        // Alterado para 'object' para aceitar new { ... }
        // Altera o parâmetro 'data' de Dictionary para 'object'
        // Altera o parâmetro de Dictionary para object
        public async Task PutAsync(string endpoint, object data)
        {
            var content = CreateJsonContent(data);
            var response = await _httpClient.PutAsync(endpoint, content);
            await EnsureSuccessAsync(response);
        }

        public async Task PatchAsync(string endpoint, object data)
        {
            var content = CreateJsonContent(data);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint) { Content = content };
            var response = await _httpClient.SendAsync(request);
            await EnsureSuccessAsync(response);
        }

        private static StringContent CreateJsonContent(object data)
        {
            var json = JsonSerializer.Serialize(data);
            return new StringContent(json, Encoding.UTF8, "application/json");
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

        public async Task<List<T>> GetListAsync<T>(string path)
        {
            var response = await _httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<T>>() ?? new List<T>();
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