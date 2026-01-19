using Fitz.Api.Controllers.Polls.UpdateVote.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Polls.UpdateVote.Persistence;

public class UpdateVote(IDbContextFactory<BotContext> contextFactory, ILogger<UpdateVote> logger) : IUpdateVote
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<UpdateVote> _logger = logger;

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

    public async Task UpdateVoteAsync(Vote vote, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating vote. VoteId: {VoteId}", vote.Id);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Votes.Update(vote);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vote updated successfully. VoteId: {VoteId}", vote.Id);
    }
}
