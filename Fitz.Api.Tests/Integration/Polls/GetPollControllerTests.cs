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

namespace Fitz.Api.Tests.Integration.Polls
{
    public class GetPollControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public GetPollControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("GetPollTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task GetPoll_WithValidId_ReturnsOk()
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

            var response = await _client.GetAsync($"/api/polls/{poll.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetPoll_WithInvalidId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/polls/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetPollByMessageId_WithValidMessageId_ReturnsOk()
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

            var response = await _client.GetAsync($"/api/polls/message/{poll.MessageId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
