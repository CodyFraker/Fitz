using Fitz.Api.Tests;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Headers;
using Xunit;
using AccountModel = Fitz.Features.Accounts.Models.Account;

namespace Fitz.Api.Tests.Integration.Account
{
    public class GetAccountControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public GetAccountControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BotContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<BotContext>(options =>
                    {
                        options.UseInMemoryDatabase("GetAccountTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task GetAccount_WithValidUserId_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var account = new AccountModel
            {
                Id = 123456789,
                Username = "TestUser",
                Beer = 100,
                LifetimeBeer = 500,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };
            
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var response = await _client.GetAsync("/api/account/123456789");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAccount_WithInvalidUserId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/account/999999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
