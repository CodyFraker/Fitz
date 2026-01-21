using Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Persistence;

public class AdminBuyFitzTickets(IDbContextFactory<BotContext> contextFactory, ILogger<AdminBuyFitzTickets> logger) : IAdminBuyFitzTickets
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<AdminBuyFitzTickets> _logger = logger;

    public async Task<LotteryEntity?> GetCurrentLotteryAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting current lottery");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var lottery = await context.Drawing
            .Where(l => l.CurrentLottery == true)
            .OrderByDescending(l => l.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (lottery != null)
        {
            _logger.LogInformation("Current lottery found. LotteryId: {LotteryId}", lottery.Id);
        }
        else
        {
            _logger.LogInformation("Current lottery not found");
        }

        return lottery;
    }
}
