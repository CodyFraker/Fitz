using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;
using Fitz.Api.Controllers.Lottery.Embeds;

namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class GetLotteryStatisticsSlashCommand(
    GetLotteryStatisticsFacade getLotteryStatisticsFacade,
    DSharpPlus.DiscordClient discordClient,
    ILogger<GetLotteryStatisticsSlashCommand> logger) : ApplicationCommandModule
{
    private readonly GetLotteryStatisticsFacade _getLotteryStatisticsFacade = getLotteryStatisticsFacade;
    private readonly DSharpPlus.DiscordClient _discordClient = discordClient;
    private readonly ILogger<GetLotteryStatisticsSlashCommand> _logger = logger;

    [SlashCommand("lotterystatistics", "View aggregated statistics from all lotteries.")]
    public async Task LotteryStatistics(InteractionContext ctx)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Lottery statistics command started via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);

        try
        {
            var command = GetLotteryStatisticsCommand.From();
            var response = await _getLotteryStatisticsFacade.Execute(command, CancellationToken.None);

            var embed = GetLotteryStatisticsEmbed.LotteryStatisticsEmbed(_discordClient, response);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral(true));

            _logger.LogInformation("Lottery statistics command completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lottery statistics command failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while retrieving lottery statistics. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
