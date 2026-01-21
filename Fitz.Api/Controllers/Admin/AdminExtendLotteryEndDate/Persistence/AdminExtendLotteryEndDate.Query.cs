using Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Persistence;

public class AdminExtendLotteryEndDate(IDbContextFactory<BotContext> contextFactory, ILogger<AdminExtendLotteryEndDate> logger) : IAdminExtendLotteryEndDate
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<AdminExtendLotteryEndDate> _logger = logger;

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
