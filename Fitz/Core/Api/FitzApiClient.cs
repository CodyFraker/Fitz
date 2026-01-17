using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;

namespace Fitz.Core.Api
{
    public class FitzApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _serviceToken;

        public FitzApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://fitz-api:8080";
            _serviceToken = Environment.GetEnvironmentVariable("API_SERVICE_TOKEN") ?? string.Empty;
            
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _serviceToken);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse?> PostAsync<TResponse>(string endpoint)
        {
            var response = await _httpClient.PostAsync(endpoint, null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            var response = await _httpClient.PatchAsJsonAsync(endpoint, request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
    }
}
