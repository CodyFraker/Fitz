using Fitz.Api.Controllers.Polls.AddVote.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Polls.AddVote.Persistence;

public class AddVote(IDbContextFactory<BotContext> contextFactory, ILogger<AddVote> logger) : IAddVote
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<AddVote> _logger = logger;

    public async Task<PollEntity?> FindPollByIdAsync(int pollId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding poll by ID. PollId: {PollId}", pollId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var poll = await context.Polls
            .Where(p => p.Id == pollId)
            .FirstOrDefaultAsync(cancellationToken);

        if (poll != null)
        {
            _logger.LogInformation("Poll found. PollId: {PollId}", pollId);
        }
        else
        {
            _logger.LogInformation("Poll not found. PollId: {PollId}", pollId);
        }

        return poll;
    }

    public async Task<PollOptionsEntity?> FindPollOptionAsync(int pollId, int optionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding poll option. PollId: {PollId}, OptionId: {OptionId}", pollId, optionId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var option = await context.PollsOptions
            .Where(o => o.PollId == pollId && o.Id == optionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (option != null)
        {
            _logger.LogInformation("Poll option found. PollId: {PollId}, OptionId: {OptionId}", pollId, optionId);
        }
        else
        {
            _logger.LogInformation("Poll option not found. PollId: {PollId}, OptionId: {OptionId}", pollId, optionId);
        }

        return option;
    }

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

    public async Task<Vote?> FindVoteAsync(int pollId, ulong userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding vote. PollId: {PollId}, UserId: {UserId}", pollId, userId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var vote = await context.Votes
            .Where(v => v.PollId == pollId && v.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (vote != null)
        {
            _logger.LogInformation("Vote found. PollId: {PollId}, UserId: {UserId}", pollId, userId);
        }
        else
        {
            _logger.LogInformation("Vote not found. PollId: {PollId}, UserId: {UserId}", pollId, userId);
        }

        return vote;
    }

    public async Task CreateVoteAsync(Vote vote, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating vote. PollId: {PollId}, UserId: {UserId}", vote.PollId, vote.UserId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Votes.Add(vote);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vote created successfully. VoteId: {VoteId}", vote.Id);
    }
}
