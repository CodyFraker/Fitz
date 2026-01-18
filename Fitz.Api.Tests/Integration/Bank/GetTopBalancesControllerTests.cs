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

namespace Fitz.Api.Tests.Integration.Bank
{
    public class GetTopBalancesControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public GetTopBalancesControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("GetTopBalancesTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task GetTopBalances_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var account1 = new AccountModel
            {
                Id = 111111111,
                Username = "User1",
                Beer = 1000,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };
            
            var account2 = new AccountModel
            {
                Id = 222222222,
                Username = "User2",
                Beer = 500,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };
            
            db.Accounts.AddRange(account1, account2);
            await db.SaveChangesAsync();

            var response = await _client.GetAsync("/api/bank/top-balances?limit=10");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
