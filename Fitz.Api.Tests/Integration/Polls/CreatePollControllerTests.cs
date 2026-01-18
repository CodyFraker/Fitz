using Fitz.Api.Tests;
using Fitz.Database;
using Fitz.Database.Entities;
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

namespace Fitz.Api.Tests.Integration.Polls
{
    public class CreatePollControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public CreatePollControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("CreatePollTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task CreatePoll_WithValidRequest_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var account = new AccountModel
            {
                Id = 123456789,
                Username = "TestUser",
                Beer = 1000,
                LifetimeBeer = 2000,
                Favorability = 50,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };
            
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var request = new
            {
                AccountId = 123456789UL,
                MessageId = 987654321UL,
                Question = "Test Question?",
                Type = PollType.YesOrNo,
                Options = new[]
                {
                    new { Answer = "Yes", EmojiName = ":white_check_mark:", EmojiId = (ulong?)0 },
                    new { Answer = "No", EmojiName = ":x:", EmojiId = (ulong?)0 }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/polls", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreatePoll_WithInvalidAccountId_ReturnsNotFound()
        {
            var request = new
            {
                AccountId = 999999999UL,
                MessageId = 987654321UL,
                Question = "Test Question?",
                Type = PollType.YesOrNo,
                Options = new[]
                {
                    new { Answer = "Yes", EmojiName = ":white_check_mark:", EmojiId = (ulong?)0 }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/polls", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
