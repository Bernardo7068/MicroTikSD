using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MikroTikSDN.Core.Exceptions;

namespace MikroTikSDN.Core
{
    public class RouterClient : IDisposable
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public string Host { get; }
        public string Name { get; set; }

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public RouterClient(string host, string username, string password, bool useHttps = false, string? name = null)
        {
            Host = host;
            Name = name ?? host;

            var scheme = useHttps ? "https" : "http";
            _baseUrl = $"{scheme}://{host}/rest";

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            _http = new HttpClient(handler);

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _http.Timeout = TimeSpan.FromSeconds(15);
        }

        public async Task<T> GetAsync<T>(string path)
        {
            var response = await _http.GetAsync($"{_baseUrl}/{path}");
            await EnsureSuccessAsync(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
        }

        public async Task<T?> PostAsync<T>(string path, object body)
        {
            var content = CreateJsonContent(body);
            var response = await _http.PostAsync($"{_baseUrl}/{path}", content);
            await EnsureSuccessAsync(response);
            var json = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(json)
                ? default
                : JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        public async Task PostAsync(string path, object body)
        {
            var content = CreateJsonContent(body);
            var response = await _http.PostAsync($"{_baseUrl}/{path}", content);
            await EnsureSuccessAsync(response);
        }

        public async Task<T?> PatchAsync<T>(string path, string id, object body)
        {
            var content = CreateJsonContent(body);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_baseUrl}/{path}/{id}")
            {
                Content = content
            };
            var response = await _http.SendAsync(request);
            await EnsureSuccessAsync(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        public async Task DeleteAsync(string path, string id)
        {
            var response = await _http.DeleteAsync($"{_baseUrl}/{path}/{id}");
            await EnsureSuccessAsync(response);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}/system/identity");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private static StringContent CreateJsonContent(object body)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
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
            _http?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}