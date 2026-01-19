using Fitz.Api.Controllers.Polls.CreatePoll.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Polls.CreatePoll.Persistence;

public class CreatePoll(IDbContextFactory<BotContext> contextFactory, ILogger<CreatePoll> logger) : ICreatePoll
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<CreatePoll> _logger = logger;

    public async Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding account by ID. UserId: {UserId}", userId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var account = await context.Accounts
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (account != null)
        {
            _logger.LogInformation("Account found. UserId: {UserId}", userId);
        }
        else
        {
            _logger.LogInformation("Account not found. UserId: {UserId}", userId);
        }

        return account;
    }

    public async Task<SettingsEntity?> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting settings");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await context.Settings
            .FirstOrDefaultAsync(cancellationToken);

        if (settings != null)
        {
            _logger.LogInformation("Settings found");
        }
        else
        {
            _logger.LogWarning("Settings not found");
        }

        return settings;
    }

    public async Task<int> GetPendingPollsCountAsync(ulong accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting pending polls count. AccountId: {AccountId}", accountId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var count = await context.Polls
            .Where(p => p.AccountId == accountId && p.Status == PollStatusEnum.Pending)
            .CountAsync(cancellationToken);

        _logger.LogInformation("Pending polls count. AccountId: {AccountId}, Count: {Count}", accountId, count);

        return count;
    }

    public async Task<PollEntity> CreatePollAsync(PollEntity poll, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating poll. AccountId: {AccountId}, Question: {Question}", poll.AccountId, poll.Question);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Polls.Add(poll);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Poll created successfully. PollId: {PollId}", poll.Id);

        return poll;
    }

    public async Task<List<PollOptionsEntity>> CreatePollOptionsAsync(int pollId, List<PollOptionsEntity> options, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating poll options. PollId: {PollId}, Count: {Count}", pollId, options.Count);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        foreach (var option in options)
        {
            option.PollId = pollId;
            context.PollsOptions.Add(option);
        }
        
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Poll options created successfully. PollId: {PollId}, Count: {Count}", pollId, options.Count);

        return options;
    }

    public async Task<PollEntity?> FindPollByMessageIdAsync(ulong messageId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding poll by message ID. MessageId: {MessageId}", messageId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var poll = await context.Polls
            .Where(p => p.MessageId == messageId)
            .OrderByDescending(p => p.SubmittedOn)
            .FirstOrDefaultAsync(cancellationToken);

        if (poll != null)
        {
            _logger.LogInformation("Poll found. PollId: {PollId}, MessageId: {MessageId}", poll.Id, messageId);
        }
        else
        {
            _logger.LogInformation("Poll not found. MessageId: {MessageId}", messageId);
        }

        return poll;
    }
}
