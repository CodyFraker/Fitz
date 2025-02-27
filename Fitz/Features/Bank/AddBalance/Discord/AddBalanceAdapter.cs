using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Features.Bank.AddBalance.Domain;
using Fitz.Features.Bank.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;

namespace Fitz.Features.Bank.AddBalance.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class AddBalanceAdapter : ApplicationCommandModule
    {
        private readonly AddBalanceService _balanceService;
        private readonly BotLog _botLog;

        public AddBalanceAdapter(AddBalanceService balanceService, BotLog botLog)
        {
            _balanceService = balanceService ?? throw new ArgumentNullException(nameof(balanceService));
            _botLog = botLog ?? throw new ArgumentNullException(nameof(botLog));
        }

        [SlashCommand("donate", "Donate beer to another user")]
        public async Task DonateCommand(
            InteractionContext ctx,
            [Option("user", "The user to donate to")] DiscordUser user,
            [Option("amount", "The amount of beer to donate")] long amount)
        {
            try
            {
                if (user.Id == ctx.User.Id)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("You cannot donate beer to yourself.")
                            .AsEphemeral(true));
                    return;
                }

                if (amount <= 0)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("You must donate a positive amount of beer.")
                            .AsEphemeral(true));
                    return;
                }

                var result = await _balanceService.TransferBalanceAsync(ctx.User.Id, user.Id, (int)amount);
                var response = AddBalanceResponse.FromResult(result);

                if (response.Success)
                {
                    _botLog.Information(LogConsoleSettings.BankLog, BankEmojis.Donate,
                        $"{ctx.User.Username} donated {amount} beer to {user.Username}");

                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"You donated {amount} beer to {user.Username}.")
                            .AsEphemeral(true));
                }
                else
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Failed to donate beer: {response.Message}")
                            .AsEphemeral(true));
                }
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.BankLog, BankEmojis.Warning,
                    $"Error donating beer: {ex.Message}");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"An error occurred: {ex.Message}")
                        .AsEphemeral(true));
            }
        }
    }
}
