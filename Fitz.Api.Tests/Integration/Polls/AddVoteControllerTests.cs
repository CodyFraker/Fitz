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
using PollModel = Fitz.Features.Polls.Models.Poll;
using PollOptionModel = Fitz.Features.Polls.Models.PollOptions;

namespace Fitz.Api.Tests.Integration.Polls
{
    public class AddVoteControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public AddVoteControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("AddVoteTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task AddVote_WithValidRequest_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var account = new AccountModel
            {
                Id = 123456789,
                Username = "TestUser",
                Beer = 100,
                LifetimeBeer = 500,
                Favorability = 50,
                CreatedDate = DateTime.UtcNow,
                LastSeenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow
            };

            var poll = new PollModel
            {
                AccountId = 111111111,
                MessageId = 987654321,
                Question = "Test Question?",
                Type = PollType.YesOrNo,
                Status = PollStatus.Approved,
                SubmittedOn = DateTime.UtcNow
            };

            var option = new PollOptionModel
            {
                PollId = 0,
                Answer = "Yes",
                EmojiName = ":white_check_mark:",
                EmojiId = 0
            };
            
            db.Accounts.Add(account);
            db.Polls.Add(poll);
            await db.SaveChangesAsync();

            option.PollId = poll.Id;
            db.PollsOptions.Add(option);
            await db.SaveChangesAsync();

            var request = new
            {
                UserId = 123456789UL,
                OptionId = option.Id
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/api/polls/{poll.Id}/vote", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
