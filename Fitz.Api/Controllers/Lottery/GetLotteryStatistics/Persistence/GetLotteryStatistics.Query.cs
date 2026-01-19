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
}
