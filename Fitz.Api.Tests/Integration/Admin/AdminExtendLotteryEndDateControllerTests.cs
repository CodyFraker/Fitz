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
using LotteryModel = Fitz.Features.Lottery.Models.Lottery;

namespace Fitz.Api.Tests.Integration.Admin
{
    public class AdminExtendLotteryEndDateControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public AdminExtendLotteryEndDateControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("AdminExtendLotteryEndDateTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task ExtendLotteryEndDate_WithValidRequest_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var lottery = new LotteryModel
            {
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(6),
                Pool = 1000,
                CurrentLottery = true,
                WinningTicket = null
            };
            
            db.Drawing.Add(lottery);
            await db.SaveChangesAsync();

            var request = new
            {
                EndDate = DateTime.UtcNow.AddDays(10)
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync("/api/admin/lottery/current/end-date", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ExtendLotteryEndDate_WithoutActiveLottery_ReturnsNotFound()
        {
            var request = new
            {
                EndDate = DateTime.UtcNow.AddDays(10)
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync("/api/admin/lottery/current/end-date", content);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
