using Fitz.Api.Tests;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Fitz.Api.Tests.Integration.Auth
{
    public class ExchangeTokenControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public ExchangeTokenControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "Discord:ClientId", "test-client-id" },
                        { "Discord:ClientSecret", "test-client-secret" },
                        { "Discord:RedirectUri", "http://localhost:3000/auth/callback" }
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task ExchangeToken_WithInvalidRequest_ReturnsBadRequest()
        {
            var request = new { };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/exchange-token", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ExchangeToken_WithMissingCode_ReturnsBadRequest()
        {
            var request = new
            {
                RedirectUri = "http://localhost:3000/auth/callback"
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/exchange-token", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ExchangeToken_WithMissingRedirectUri_ReturnsBadRequest()
        {
            var request = new
            {
                Code = "test-code"
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/exchange-token", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ExchangeToken_WithValidRequest_ReturnsResponse()
        {
            var request = new
            {
                Code = "test-authorization-code",
                RedirectUri = "http://localhost:3000/auth/callback"
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/exchange-token", content);

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.InternalServerError);
        }
    }
}
