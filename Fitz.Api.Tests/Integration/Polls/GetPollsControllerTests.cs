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
    public class GetPollsControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public GetPollsControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("GetPollsTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task GetPolls_WithoutFilters_ReturnsOk()
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

            var response = await _client.GetAsync("/api/polls");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetPolls_WithStatusFilter_ReturnsOk()
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

            var response = await _client.GetAsync("/api/polls?status=Pending");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
