using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Features.Bank;
using Fitz.Features.Polls.Create.Domain;
using Fitz.Features.Polls.Create.Persistance;
using Fitz.Features.Polls.Models;
using Fitz.Features.Polls.Update.Domain;
using Fitz.Features.Polls.Update.Persistance;
using Fitz.Features.Polls.Vote.Domain;
using Fitz.Features.Polls.Vote.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fitz.Features.Polls.Tests
{
    public class PollsTests
    {
        private readonly Mock<BotLog> _mockBotLog;
        private readonly Mock<BankService> _mockBankService;
        private readonly DbContextOptions<BotContext> _dbContextOptions;
        private readonly ServiceProvider _serviceProvider;

        public PollsTests()
        {
            // Setup mock logger and bank service
            _mockBotLog = new Mock<BotLog>();
            _mockBankService = new Mock<BankService>();

            // Setup in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<BotContext>()
                .UseInMemoryDatabase(databaseName: $"PollsTestDb_{Guid.NewGuid()}")
                .Options;

            // Setup service provider
            var services = new ServiceCollection();
            services.AddSingleton(_mockBotLog.Object);
            services.AddSingleton(_mockBankService.Object);
            services.AddDbContext<BotContext>(options => options.UseInMemoryDatabase(databaseName: $"PollsTestDb_{Guid.NewGuid()}"));
            services.AddTransient<CreatePollRepository>();
            services.AddTransient<CreatePollService>();
            services.AddTransient<UpdatePollRepository>();
            services.AddTransient<UpdatePollService>();
            services.AddTransient<VoteRepository>();
            services.AddTransient<VoteService>();

            _serviceProvider = services.BuildServiceProvider();

            // Setup the database with required tables
            using var scope = _serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<BotContext>();
            context.Database.EnsureCreated();
        }

        [Fact]
        public async Task CreatePoll_ShouldCreateNewPoll()
        {
            // Arrange
            var createPollService = _serviceProvider.GetRequiredService<CreatePollService>();
            var options = new List<string> { "Option 1", "Option 2", "Option 3" };
            var command = new CreatePollCommand(
                accountId: 123456789,
                channelId: 987654321,
                title: "Test Poll",
                description: "This is a test poll",
                options: options,
                pollType: PollType.Standard,
                endDate: DateTime.UtcNow.AddDays(1),
                allowMultipleVotes: false);

            // Setup bank service mock
            _mockBankService
                .Setup(x => x.UserSubmittedPollPenalty(It.IsAny<ulong>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await createPollService.CreatePollAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Poll", result.Title);
            Assert.Equal("This is a test poll", result.Description);
            Assert.Equal(3, result.Options.Values.Count);
            Assert.Equal(PollStatus.Active, result.Status);
            Assert.Equal(123456789UL, result.AccountId);

            // Verify bank service was called
            _mockBankService.Verify(x => x.UserSubmittedPollPenalty(123456789UL), Times.Once);
        }

        [Fact]
        public async Task UpdatePoll_ShouldUpdatePollStatus()
        {
            // Arrange
            var createPollService = _serviceProvider.GetRequiredService<CreatePollService>();
            var updatePollService = _serviceProvider.GetRequiredService<UpdatePollService>();

            // First create a poll
            var options = new List<string> { "Option 1", "Option 2" };
            var createCommand = new CreatePollCommand(
                accountId: 123456789,
                channelId: 987654321,
                title: "Test Poll",
                description: "This is a test poll",
                options: options,
                pollType: PollType.Standard,
                endDate: DateTime.UtcNow.AddDays(1),
                allowMultipleVotes: false);

            _mockBankService
                .Setup(x => x.UserSubmittedPollPenalty(It.IsAny<ulong>()))
                .Returns(Task.CompletedTask);

            var poll = await createPollService.CreatePollAsync(createCommand);

            // Now update the poll
            var updateCommand = new UpdatePollCommand(
                pollId: poll.Id,
                status: PollStatus.Closed,
                userId: 123456789);

            // Act
            var result = await updatePollService.UpdatePollStatusAsync(updateCommand);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(poll.Id, result.Id);
            Assert.Equal(PollStatus.Closed, result.Status);
        }

        [Fact]
        public async Task Vote_ShouldAddVoteToPoll()
        {
            // Arrange
            var createPollService = _serviceProvider.GetRequiredService<CreatePollService>();
            var voteService = _serviceProvider.GetRequiredService<VoteService>();
            var voteRepository = _serviceProvider.GetRequiredService<VoteRepository>();

            // First create a poll
            var options = new List<string> { "Option 1", "Option 2" };
            var createCommand = new CreatePollCommand(
                accountId: 123456789,
                channelId: 987654321,
                title: "Test Poll",
                description: "This is a test poll",
                options: options,
                pollType: PollType.Standard,
                endDate: DateTime.UtcNow.AddDays(1),
                allowMultipleVotes: false);

            _mockBankService
                .Setup(x => x.UserSubmittedPollPenalty(It.IsAny<ulong>()))
                .Returns(Task.CompletedTask);

            var poll = await createPollService.CreatePollAsync(createCommand);

            // Now vote on the poll
            var voteCommand = new VoteCommand(
                pollId: poll.Id,
                userId: 987654321, // Different user from the creator
                optionIndex: 0);

            // Act
            var result = await voteService.VoteAsync(voteCommand);

            // Assert
            Assert.True(result);

            // Verify the vote was added to the database
            var vote = await voteRepository.GetVoteAsync(poll.Id, 987654321);
            Assert.NotNull(vote);
            Assert.Equal(poll.Id, vote.PollId);
            Assert.Equal(9876543212L, vote.Id);
            Assert.Equal(9876543212L, vote.Id);
        }
    }
}