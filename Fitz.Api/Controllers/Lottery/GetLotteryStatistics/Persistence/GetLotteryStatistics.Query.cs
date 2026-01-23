using Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Persistence;

public class GetLotteryStatistics(IDbContextFactory<BotContext> contextFactory, ILogger<GetLotteryStatistics> logger) : IGetLotteryStatistics
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetLotteryStatistics> _logger = logger;

    public async Task<List<LotteryEntity>> FindAllLotteriesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding all lotteries");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var lotteries = await context.Drawing
            .OrderBy(x => x.StartDate)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("All lotteries found. Count: {Count}", lotteries.Count);

        return lotteries;
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

    public async Task<List<WinnersEntity>> GetAllWinnersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all winners");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var winners = await context.Winners
            .ToListAsync(cancellationToken);

        _logger.LogInformation("All winners found. Count: {Count}", winners.Count);

        return winners;
    }

    public async Task<int> GetTicketCountForWinnerAsync(int drawingId, ulong accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting ticket count for winner. DrawingId: {DrawingId}, AccountId: {AccountId}", drawingId, accountId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var ticketCount = await context.Ticket
            .Where(x => x.Drawing == drawingId && x.AccountId == accountId)
            .CountAsync(cancellationToken);

        _logger.LogInformation("Ticket count for winner. DrawingId: {DrawingId}, AccountId: {AccountId}, TicketCount: {TicketCount}", drawingId, accountId, ticketCount);

        return ticketCount;
    }
}
