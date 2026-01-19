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
    public class GetPollOptionsControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public GetPollOptionsControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("GetPollOptionsTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task GetPollOptions_WithValidId_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var poll = new PollModel
            {
                AccountId = 123456789,
                MessageId = 987654321,
                Question = "Test Question?",
                Type = PollTypeEnum.YesOrNo,
                Status = PollStatusEnum.Approved,
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

            var response = await _client.GetAsync($"/api/polls/{poll.Id}/options");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
