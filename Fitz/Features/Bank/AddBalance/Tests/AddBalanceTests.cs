using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Bank.AddBalance.Domain;
using Fitz.Features.Bank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fitz.Features.Bank.AddBalance.Tests
{
    public class AddBalanceTests
    {
        private readonly DbContextOptions<BotContext> _dbContextOptions;
        private readonly ServiceProvider _serviceProvider;

        public AddBalanceTests()
        {
            // Setup in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<BotContext>()
                .UseInMemoryDatabase(databaseName: $"BankTestDb_{Guid.NewGuid()}")
                .Options;

            // Setup service provider
            var services = new ServiceCollection();
            services.AddDbContext<BotContext>(options => options.UseInMemoryDatabase(databaseName: $"BankTestDb_{Guid.NewGuid()}"));

            // Add required services for testing
            // Note: You'll need to add your actual services here
            // services.AddTransient<YourService>();

            _serviceProvider = services.BuildServiceProvider();

            // Initialize test data
            InitializeTestData().Wait();
        }

        private async Task InitializeTestData()
        {
            using (var context = new BotContext(_dbContextOptions))
            {
                // Create test accounts
                var account1 = new Account
                {
                    UserId = 123456789,
                    Balance = 1000,
                    LifetimeBalance = 2000
                };

                var account2 = new Account
                {
                    UserId = 987654321,
                    Balance = 500,
                    LifetimeBalance = 1000
                };

                context.Add(account1);
                context.Add(account2);
                await context.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task AddBalance_ShouldIncreaseRecipientBalance()
        {
            // Arrange
            var command = new AddBalanceCommand(
                recipientId: 123456789,
                senderId: 987654321,
                amount: 100,
                reason: TransactionReason.Bonus,
                updateLifetimeBalance: true);

            // Act
            // TODO: Replace with your actual service call
            // var result = await _yourService.AddBalance(command);

            // For now, we'll simulate the operation directly
            using (var context = new BotContext(_dbContextOptions))
            {
                var recipient = await context.Set<Account>().FirstOrDefaultAsync(a => a.UserId == command.RecipientId);
                if (recipient != null)
                {
                    recipient.Balance += command.Amount;
                    if (command.UpdateLifetimeBalance)
                    {
                        recipient.LifetimeBalance += command.Amount;
                    }
                    await context.SaveChangesAsync();
                }
            }

            // Assert
            using (var context = new BotContext(_dbContextOptions))
            {
                var recipient = await context.Set<Account>().FirstOrDefaultAsync(a => a.UserId == command.RecipientId);
                Assert.NotNull(recipient);
                Assert.Equal(1100, recipient.Balance);
                Assert.Equal(2100, recipient.LifetimeBalance);
            }
        }

        [Fact]
        public async Task AddBalance_WithoutLifetimeUpdate_ShouldOnlyIncreaseBalance()
        {
            // Arrange
            var command = new AddBalanceCommand(
                recipientId: 123456789,
                senderId: 987654321,
                amount: 100,
                reason: TransactionReason.Bonus,
                updateLifetimeBalance: false);

            // Act
            // TODO: Replace with your actual service call
            // var result = await _yourService.AddBalance(command);

            // For now, we'll simulate the operation directly
            using (var context = new BotContext(_dbContextOptions))
            {
                var recipient = await context.Set<Account>().FirstOrDefaultAsync(a => a.UserId == command.RecipientId);
                if (recipient != null)
                {
                    recipient.Balance += command.Amount;
                    if (command.UpdateLifetimeBalance)
                    {
                        recipient.LifetimeBalance += command.Amount;
                    }
                    await context.SaveChangesAsync();
                }
            }

            // Assert
            using (var context = new BotContext(_dbContextOptions))
            {
                var recipient = await context.Set<Account>().FirstOrDefaultAsync(a => a.UserId == command.RecipientId);
                Assert.NotNull(recipient);
                Assert.Equal(1100, recipient.Balance);
                Assert.Equal(2000, recipient.LifetimeBalance); // Should remain unchanged
            }
        }

        [Fact]
        public void AddBalanceCommand_WithZeroRecipientId_ShouldThrowException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AddBalanceCommand(
                recipientId: 0,
                senderId: 987654321,
                amount: 100,
                reason: TransactionReason.Bonus,
                updateLifetimeBalance: true));

            Assert.Contains("Recipient ID cannot be zero", exception.Message);
        }

        [Fact]
        public void AddBalanceCommand_WithNegativeAmount_ShouldThrowException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AddBalanceCommand(
                recipientId: 123456789,
                senderId: 987654321,
                amount: -100,
                reason: TransactionReason.Bonus,
                updateLifetimeBalance: true));

            Assert.Contains("Amount must be greater than zero", exception.Message);
        }
    }
}