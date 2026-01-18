using Fitz.Api.Tests;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using AccountModel = Fitz.Features.Accounts.Models.Account;
using ApiProgram = Fitz.Api.Program;

namespace Fitz.Api.Tests.Integration.Account
{
    public class SetTicketAmountControllerTests : IClassFixture<WebApplicationFactory<ApiProgram>>
    {
        private readonly WebApplicationFactory<ApiProgram> _factory;
        private readonly HttpClient _client;
        private readonly ulong _testUserId = 104359875206725632;

        public SetTicketAmountControllerTests(WebApplicationFactory<ApiProgram> factory)
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
                        options.UseInMemoryDatabase("SetTicketAmountTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task SetTicketAmount_WithValidRequest_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var account = new AccountModel
            {
                Id = _testUserId,
                Username = "TestUser",
                Beer = 100,
                LifetimeBeer = 500,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow,
                SubscribeTickets = 5
            };
            
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var request = new
            {
                UserId = _testUserId,
                Amount = 10
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/account/ticket-amount", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var verifyScope = _factory.Services.CreateScope();
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<BotContext>();
            var updatedAccount = verifyDb.Accounts.Find(_testUserId);
            Assert.NotNull(updatedAccount);
            Assert.Equal(10, updatedAccount.SubscribeTickets);
        }

        [Fact]
        public async Task SetTicketAmount_WithInvalidUserId_ReturnsNotFound()
        {
            var request = new
            {
                UserId = 999999999UL,
                Amount = 10
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/account/ticket-amount", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SetTicketAmount_WithInvalidRequest_ReturnsBadRequest()
        {
            var request = new
            {
                UserId = _testUserId,
                Amount = -1
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/account/ticket-amount", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SetTicketAmount_WithoutAuth_ReturnsUnauthorized()
        {
            var clientWithoutAuth = _factory.CreateClient();
            
            var request = new
            {
                UserId = _testUserId,
                Amount = 10
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await clientWithoutAuth.PostAsync("/api/account/ticket-amount", content);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
