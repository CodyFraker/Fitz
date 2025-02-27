using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Lottery.Create.Domain;
using Fitz.Features.Lottery.Create.Persistance;
using Fitz.Features.Lottery.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fitz.Features.Lottery.Tests
{
    public class CreateLotteryTests
    {
        private readonly Mock<BotLog> _mockBotLog;
        private readonly DbContextOptions<BotContext> _dbContextOptions;
        private readonly ServiceProvider _serviceProvider;

        public CreateLotteryTests()
        {
            // Setup mock logger
            _mockBotLog = new Mock<BotLog>();

            // Setup in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<BotContext>()
                .UseInMemoryDatabase(databaseName: $"LotteryTestDb_{Guid.NewGuid()}")
                .Options;

            // Setup service provider
            var services = new ServiceCollection();
            services.AddSingleton(_mockBotLog.Object);
            services.AddDbContext<BotContext>(options => options.UseInMemoryDatabase(databaseName: $"LotteryTestDb_{Guid.NewGuid()}"));
            services.AddTransient<CreateLotteryService>();
            services.AddTransient<CreateLotteryRepository>();
            services.AddTransient<CreateLotteryConductor>();

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task CreateLottery_ShouldCreateNewLottery()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);
            var initialPool = 1000;
            var command = new CreateLotteryCommand(startDate, endDate, initialPool);

            var conductor = _serviceProvider.GetRequiredService<CreateLotteryConductor>();

            // Act
            var result = await conductor.CreateLottery(command);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            var lottery = result.Data as Models.Lottery;
            Assert.NotNull(lottery);
            Assert.Equal(startDate, lottery.StartDate);
            Assert.Equal(endDate, lottery.EndDate);
            Assert.Equal(initialPool, lottery.Pool);
            Assert.True(lottery.CurrentLottery);
        }

        [Fact]
        public async Task CreateLottery_WithExistingCurrentLottery_ShouldUpdatePreviousLottery()
        {
            // Arrange
            using (var context = new BotContext(_dbContextOptions))
            {
                // Add an existing current lottery
                var existingLottery = new Models.Lottery
                {
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    EndDate = DateTime.UtcNow.AddDays(-1),
                    Pool = 500,
                    CurrentLottery = true
                };
                context.Add(existingLottery);
                await context.SaveChangesAsync();
            }

            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);
            var initialPool = 1000;
            var command = new CreateLotteryCommand(startDate, endDate, initialPool);

            var conductor = _serviceProvider.GetRequiredService<CreateLotteryConductor>();

            // Act
            var result = await conductor.CreateLottery(command);

            // Assert
            Assert.True(result.Success);

            // Check that the previous lottery is no longer current
            using (var context = new BotContext(_dbContextOptions))
            {
                var lotteries = await context.Set<Models.Lottery>().ToListAsync();
                Assert.Equal(2, lotteries.Count);

                var previousLottery = lotteries.FirstOrDefault(l => l.Pool == 500);
                Assert.NotNull(previousLottery);
                Assert.False(previousLottery.CurrentLottery);

                var newLottery = lotteries.FirstOrDefault(l => l.Pool == 1000);
                Assert.NotNull(newLottery);
                Assert.True(newLottery.CurrentLottery);
            }
        }

        [Fact]
        public async Task GetCurrentLottery_WithNoLottery_ShouldReturnFailure()
        {
            // Arrange
            var conductor = _serviceProvider.GetRequiredService<CreateLotteryConductor>();

            // Act
            var result = await conductor.GetCurrentLottery();

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Contains("No active lottery found", result.Message);
        }

        [Fact]
        public async Task GetCurrentLottery_WithExistingLottery_ShouldReturnLottery()
        {
            // Arrange
            using (var context = new BotContext(_dbContextOptions))
            {
                // Add an existing current lottery
                var existingLottery = new Models.Lottery
                {
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(6),
                    Pool = 500,
                    CurrentLottery = true
                };
                context.Add(existingLottery);
                await context.SaveChangesAsync();
            }

            var conductor = _serviceProvider.GetRequiredService<CreateLotteryConductor>();

            // Act
            var result = await conductor.GetCurrentLottery();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            var lottery = result.Data as Models.Lottery;
            Assert.NotNull(lottery);
            Assert.Equal(500, lottery.Pool);
            Assert.True(lottery.CurrentLottery);
        }
    }
}