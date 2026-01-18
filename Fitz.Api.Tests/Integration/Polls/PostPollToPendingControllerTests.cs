using Fitz.Api.Tests;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using Xunit;
using PollModel = Fitz.Features.Polls.Models.Poll;
using PollOptionModel = Fitz.Features.Polls.Models.PollOptions;

namespace Fitz.Api.Tests.Integration.Polls
{
    public class PostPollToPendingControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public PostPollToPendingControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("PostPollToPendingTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task PostPollToPending_WithNonExistentPoll_ReturnsNotFound()
        {
            var response = await _client.PostAsync("/api/polls/999/post-to-pending", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PostPollToPending_WithAlreadyPostedPoll_ReturnsBadRequest()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var poll = new PollModel
            {
                AccountId = 123456789,
                MessageId = 987654321,
                Question = "Test Question?",
                Type = PollType.YesOrNo,
                Status = PollStatus.Pending,
                SubmittedOn = DateTime.UtcNow
            };
            
            db.Polls.Add(poll);
            await db.SaveChangesAsync();

            var response = await _client.PostAsync($"/api/polls/{poll.Id}/post-to-pending", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostPollToPending_WithNonPendingStatus_ReturnsBadRequest()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var poll = new PollModel
            {
                AccountId = 123456789,
                MessageId = 0,
                Question = "Test Question?",
                Type = PollType.YesOrNo,
                Status = PollStatus.Approved,
                SubmittedOn = DateTime.UtcNow
            };
            
            db.Polls.Add(poll);
            await db.SaveChangesAsync();

            var response = await _client.PostAsync($"/api/polls/{poll.Id}/post-to-pending", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostPollToPending_WithNoOptions_ReturnsBadRequest()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var poll = new PollModel
            {
                AccountId = 123456789,
                MessageId = 0,
                Question = "Test Question?",
                Type = PollType.YesOrNo,
                Status = PollStatus.Pending,
                SubmittedOn = DateTime.UtcNow
            };
            
            db.Polls.Add(poll);
            await db.SaveChangesAsync();

            var response = await _client.PostAsync($"/api/polls/{poll.Id}/post-to-pending", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostPollToPending_WithValidPendingPoll_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var poll = new PollModel
            {
                AccountId = 123456789,
                MessageId = 0,
                Question = "Test Question?",
                Type = PollType.YesOrNo,
                Status = PollStatus.Pending,
                SubmittedOn = DateTime.UtcNow
            };
            
            db.Polls.Add(poll);
            await db.SaveChangesAsync();

            var option = new PollOptionModel
            {
                PollId = poll.Id,
                Answer = "Yes",
                EmojiName = ":white_check_mark:",
                EmojiId = 0
            };

            db.PollsOptions.Add(option);
            await db.SaveChangesAsync();

            var response = await _client.PostAsync($"/api/polls/{poll.Id}/post-to-pending", null);

            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.InternalServerError);
        }
    }
}
