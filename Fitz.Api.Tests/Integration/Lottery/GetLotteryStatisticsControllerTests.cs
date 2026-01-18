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
using LotteryModel = Fitz.Features.Lottery.Models.Lottery;

namespace Fitz.Api.Tests.Integration.Lottery
{
    public class GetLotteryStatisticsControllerTests : IClassFixture<WebApplicationFactory<Fitz.Api.Program>>
    {
        private readonly WebApplicationFactory<Fitz.Api.Program> _factory;
        private readonly HttpClient _client;

        public GetLotteryStatisticsControllerTests(WebApplicationFactory<Fitz.Api.Program> factory)
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
                        options.UseInMemoryDatabase("GetLotteryStatisticsTestDb");
                    });

                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
                });
            });

            _client = _factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        }

        [Fact]
        public async Task GetLotteryStatistics_WithLotteries_ReturnsOk()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BotContext>();
            
            var lottery1 = new LotteryModel
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(-23),
                Pool = 500,
                CurrentLottery = false,
                WinningTicket = 123
            };
            
            var lottery2 = new LotteryModel
            {
                StartDate = DateTime.UtcNow.AddDays(-15),
                EndDate = DateTime.UtcNow.AddDays(-8),
                Pool = 750,
                CurrentLottery = false,
                WinningTicket = 456
            };
            
            var lottery3 = new LotteryModel
            {
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(6),
                Pool = 1000,
                CurrentLottery = true,
                WinningTicket = null
            };
            
            db.Drawing.Add(lottery1);
            db.Drawing.Add(lottery2);
            db.Drawing.Add(lottery3);
            await db.SaveChangesAsync();

            var response = await _client.GetAsync("/api/lottery/statistics");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetLotteryStatistics_WithoutLotteries_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/lottery/statistics");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
