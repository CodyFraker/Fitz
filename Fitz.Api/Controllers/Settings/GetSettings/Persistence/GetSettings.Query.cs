using Fitz.Api.Controllers.Settings.GetSettings.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Settings.GetSettings.Persistence;

public class GetSettings(IDbContextFactory<BotContext> contextFactory, ILogger<GetSettings> logger) : IGetSettings
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetSettings> _logger = logger;

    public async Task<SettingsEntity> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting settings");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var settings = await context.Settings.FirstOrDefaultAsync(cancellationToken);

        if (settings == null)
        {
            _logger.LogInformation("Settings not found, creating base settings");
            settings = CreateBaseSettings(context);
            context.Settings.Add(settings);
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Base settings created successfully");
        }

        _logger.LogInformation("Settings retrieved. SettingsId: {SettingsId}", settings.Id);

        return settings;
    }

    private static SettingsEntity CreateBaseSettings(BotContext context)
    {
        return new SettingsEntity
        {
            LotteryDuration = 7,
            BaseLotteryPool = 36,
            LotteryPoolRollover = true,
            TicketCost = 1,
            MaxTickets = 128,
            AccountCreationBonusAmount = 128,
            BaseHappyHourAmount = 6,
            RenameBaseCost = 6,
            PollApprovedBonus = 24,
            PollSubmittedPenalty = 36,
            PollDeclinedPenalty = 0,
            PollVote = 12,
            PollCreatorTip = 6,
            MaxPendingPolls = 10,
            FavorabilityBeerRatioThreshold = 2.0m,
            FavorabilityLowThreshold = 10,
            FavorabilityBaseDropPercent = 1.0m,
            FavorabilityDropMultiplier = 1.5m
        };
    }
}
