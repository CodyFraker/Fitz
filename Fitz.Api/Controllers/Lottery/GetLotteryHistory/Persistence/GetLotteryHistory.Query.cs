using Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Lottery.GetLotteryHistory.Persistence;

public class GetLotteryHistory(IDbContextFactory<BotContext> contextFactory, ILogger<GetLotteryHistory> logger) : IGetLotteryHistory
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetLotteryHistory> _logger = logger;

    public async Task<List<LotteryEntity>> FindHistoryAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding lottery history. Skip: {Skip}, Take: {Take}", skip, take);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var lotteries = await context.Drawing
            .Where(x => x.CurrentLottery == false)
            .OrderByDescending(x => x.EndDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Lottery history found. Skip: {Skip}, Take: {Take}, Count: {Count}", skip, take, lotteries.Count);

        return lotteries;
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting total count of past lotteries");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var totalCount = await context.Drawing
            .Where(x => x.CurrentLottery == false)
            .CountAsync(cancellationToken);

        _logger.LogInformation("Total count of past lotteries: {TotalCount}", totalCount);

        return totalCount;
    }

    public async Task<int> GetTotalTicketsAsync(int lotteryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting total tickets for lottery. LotteryId: {LotteryId}", lotteryId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var totalTickets = await context.Ticket
            .Where(x => x.Drawing == lotteryId)
            .CountAsync(cancellationToken);

        _logger.LogInformation("Total tickets for lottery. LotteryId: {LotteryId}, TotalTickets: {TotalTickets}", lotteryId, totalTickets);

        return totalTickets;
    }

    public async Task<int> GetTotalParticipantsAsync(int lotteryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting total participants for lottery. LotteryId: {LotteryId}", lotteryId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var totalParticipants = await context.Ticket
            .Where(x => x.Drawing == lotteryId)
            .Select(x => x.AccountId)
            .Distinct()
            .CountAsync(cancellationToken);

        _logger.LogInformation("Total participants for lottery. LotteryId: {LotteryId}, TotalParticipants: {TotalParticipants}", lotteryId, totalParticipants);

        return totalParticipants;
    }

    public async Task<List<(WinnersEntity Winner, AccountEntity Account)>> GetWinnersByDrawingIdAsync(int drawingId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting winners for lottery. DrawingId: {DrawingId}", drawingId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var winners = await context.Winners
            .Where(x => x.Drawing == drawingId)
            .Join(
                context.Accounts,
                winner => winner.AccountId,
                account => account.Id,
                (winner, account) => new { Winner = winner, Account = account }
            )
            .Select(x => new { x.Winner, x.Account })
            .ToListAsync(cancellationToken);

        var result = winners.Select(x => (x.Winner, x.Account)).ToList();

        _logger.LogInformation("Winners found for lottery. DrawingId: {DrawingId}, Count: {Count}", drawingId, result.Count);

        return result;
    }
}
