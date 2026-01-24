using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.GetAccount.Domain;
using Fitz.Api.Controllers.Bank.GetTransactions.Domain;
using Fitz.Variables.Emojis;
using ToMarkdownTable;
using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.GetTransactions.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class GetTransactionsSlashCommand(
    GetTransactionsFacade getTransactionsFacade,
    GetAccountFacade getAccountFacade,
    DiscordClient discordClient,
    ILogger<GetTransactionsSlashCommand> logger) : ApplicationCommandModule
{
    private readonly GetTransactionsFacade _getTransactionsFacade = getTransactionsFacade;
    private readonly GetAccountFacade _getAccountFacade = getAccountFacade;
    private readonly DiscordClient _discordClient = discordClient;
    private readonly ILogger<GetTransactionsSlashCommand> _logger = logger;

    [SlashCommand("transactions", "Get the last 10 transactions")]
    public async Task GetLastTransactions(InteractionContext ctx)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Transactions command started via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);

        try
        {
            var command = GetTransactionsCommand.From(10);
            var response = await _getTransactionsFacade.Execute(command, CancellationToken.None);

            var transactionData = new List<object>();
            foreach (var transaction in response.Transactions)
            {
                try
                {
                    var accountCommand = GetAccountCommand.From(transaction.Sender);
                    var accountResponse = await _getAccountFacade.Execute(accountCommand, CancellationToken.None);
                    transactionData.Add(new
                    {
                        User = accountResponse.Username ?? "Unknown",
                        Beer = transaction.Amount,
                        Type = transaction.Reason,
                        Date = transaction.Timestamp.ToShortDateString()
                    });
                }
                catch
                {
                    transactionData.Add(new
                    {
                        User = "Unknown",
                        Beer = transaction.Amount,
                        Type = transaction.Reason,
                        Date = transaction.Timestamp.ToShortDateString()
                    });
                }
            }

            string table = transactionData.ToMarkdownTable();

            DiscordEmbedBuilder transactionEmbed = new()
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
                .AddEmbed(transactionEmbed.Build())
                .AsEphemeral(true));

            _logger.LogInformation("Transactions command completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}, Count: {Count}", userId, username, response.Transactions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transactions command failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while retrieving transactions. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
