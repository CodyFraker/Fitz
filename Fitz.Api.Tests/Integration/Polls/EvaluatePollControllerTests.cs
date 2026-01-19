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
using PollModel = Fitz.Features.Polls.Models.Poll;

namespace Fitz.Api.Tests.Integration.Polls
{
    public class EvaluatePollControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public EvaluatePollControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("EvaluatePollTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task EvaluatePoll_WithValidRequest_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var poll = new PollModel
            {
                AccountId = 123456789,
                MessageId = 987654321,
                Question = "Test Question?",
                Type = PollTypeEnum.YesOrNo,
                Status = PollStatusEnum.Pending,
                SubmittedOn = DateTime.UtcNow
            };
            
            db.Polls.Add(poll);
            await db.SaveChangesAsync();

            var request = new
            {
                Status = PollStatusEnum.Approved
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync($"/api/polls/{poll.Id}/evaluate", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task EvaluatePoll_WithInvalidId_ReturnsNotFound()
        {
            var request = new
            {
                Status = PollStatusEnum.Approved
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync("/api/polls/99999/evaluate", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
