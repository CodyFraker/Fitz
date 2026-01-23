using Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Lottery.GetCurrentLottery.Persistence;

public class GetCurrentLottery(IDbContextFactory<BotContext> contextFactory, ILogger<GetCurrentLottery> logger) : IGetCurrentLottery
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetCurrentLottery> _logger = logger;

    public async Task<LotteryEntity?> FindCurrentAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding current lottery");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var lottery = await context.Drawing
            .Where(x => x.CurrentLottery == true)
            .FirstOrDefaultAsync(cancellationToken);

        if (lottery != null)
        {
            _logger.LogInformation("Current lottery found. LotteryId: {LotteryId}", lottery.Id);
        }
        else
        {
            _logger.LogInformation("No current lottery found");
        }

        return lottery;
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

    public async Task<int> GetLastWinningTicketAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting last winning ticket");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var lastLottery = await context.Drawing
            .Where(x => x.CurrentLottery == false)
            .OrderByDescending(x => x.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastLottery == null)
        {
            _logger.LogInformation("No previous lottery found, returning 0");
            return 0;
        }

        var winningTicket = lastLottery.WinningTicket ?? 0;
        _logger.LogInformation("Last winning ticket found. WinningTicket: {WinningTicket}", winningTicket);

        return winningTicket;
    }
}
