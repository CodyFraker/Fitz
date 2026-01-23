using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain;
using Fitz.Api.Controllers.Lottery.Embeds;

namespace Fitz.Api.Controllers.Lottery.GetLotteryHistory.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class GetLotteryHistorySlashCommand(
    GetLotteryHistoryFacade getLotteryHistoryFacade,
    DSharpPlus.DiscordClient discordClient,
    ILogger<GetLotteryHistorySlashCommand> logger) : ApplicationCommandModule
{
    private readonly GetLotteryHistoryFacade _getLotteryHistoryFacade = getLotteryHistoryFacade;
    private readonly DSharpPlus.DiscordClient _discordClient = discordClient;
    private readonly ILogger<GetLotteryHistorySlashCommand> _logger = logger;

    [SlashCommand("lotteryhistory", "View the past 5 lottery results.")]
    public async Task LotteryHistory(InteractionContext ctx)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Lottery history command started via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);

        try
        {
            var command = GetLotteryHistoryCommand.From(0, 5);
            var response = await _getLotteryHistoryFacade.Execute(command, CancellationToken.None);

            var embed = GetLotteryHistoryEmbed.LotteryHistoryEmbed(_discordClient, response);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(embed)
                .AsEphemeral(true));

            _logger.LogInformation("Lottery history command completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lottery history command failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while retrieving lottery history. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
