using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Bank.GetTopBalances.Domain;
using Fitz.Variables.Emojis;
using ToMarkdownTable;

namespace Fitz.Api.Controllers.Bank.GetTopBalances.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class GetTopBalancesSlashCommand(
    GetTopBalancesFacade getTopBalancesFacade,
    DiscordClient discordClient,
    ILogger<GetTopBalancesSlashCommand> logger) : ApplicationCommandModule
{
    private readonly GetTopBalancesFacade _getTopBalancesFacade = getTopBalancesFacade;
    private readonly DiscordClient _discordClient = discordClient;
    private readonly ILogger<GetTopBalancesSlashCommand> _logger = logger;

    [SlashCommand("topbalances", "Get the top 10 balances for all users.")]
    public async Task TopBalances(InteractionContext ctx)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Top balances command started via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);

        try
        {
            var command = GetTopBalancesCommand.From(10);
            var response = await _getTopBalancesFacade.Execute(command, CancellationToken.None);

            string table = response.Accounts.Select(account => new
            {
                User = account.Username,
                Beer = account.Beer + " "
            }).ToMarkdownTable();

            DiscordEmbedBuilder balanceEmbed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(_discordClient, LotteryEmojis.Ticket).Url,
                    Text = $"Bank",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Description = $"```md\n{table}\n```",
            };

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(balanceEmbed.Build())
                .AsEphemeral(true));

            _logger.LogInformation("Top balances command completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}, Count: {Count}", userId, username, response.Accounts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Top balances command failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while retrieving top balances. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
