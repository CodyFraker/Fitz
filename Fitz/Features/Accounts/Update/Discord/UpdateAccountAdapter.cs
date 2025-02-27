using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Features.Accounts.Update.Domain;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Update.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class UpdateAccountAdapter : ApplicationCommandModule
    {
        private readonly UpdateAccountService _updateService;
        private readonly BotLog _botLog;

        public UpdateAccountAdapter(UpdateAccountService updateService, BotLog botLog)
        {
            _updateService = updateService;
            _botLog = botLog;
        }

        [SlashCommand("update-safe-balance", "Update your safe balance for lottery participation")]
        public async Task UpdateSafeBalance(InteractionContext ctx, [Option("amount", "The amount of beer to keep safe")] long amount)
        {
            await UpdateAccountProperty(ctx, ctx.User.Id, "SafeBalance", (int)amount);
        }

        [SlashCommand("update-lottery-subscription", "Subscribe or unsubscribe from the lottery")]
        public async Task UpdateLotterySubscription(InteractionContext ctx, [Option("subscribe", "Whether to subscribe to the lottery")] bool subscribe)
        {
            await UpdateAccountProperty(ctx, ctx.User.Id, "SubscribeToLottery", subscribe);
        }

        [SlashCommand("update-ticket-amount", "Update the number of tickets to buy for each lottery")]
        public async Task UpdateTicketAmount(InteractionContext ctx, [Option("amount", "The number of tickets to buy")] long amount)
        {
            if (amount < 1)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Ticket amount must be at least 1.").AsEphemeral(true));
                return;
            }

            await UpdateAccountProperty(ctx, ctx.User.Id, "SubscribeTickets", (int)amount);
        }

        private async Task UpdateAccountProperty(InteractionContext ctx, ulong userId, string propertyName, object propertyValue)
        {
            try
            {
                UpdateAccountCommand command = new UpdateAccountCommand { Id = userId };

                // Set the appropriate property based on the property name
                switch (propertyName)
                {
                    case "Username":
                        command.Username = propertyValue as string;
                        break;
                    case "SafeBalance":
                        command.SafeBalance = (int)propertyValue;
                        break;
                    case "SubscribeToLottery":
                        command.SubscribeToLottery = (bool)propertyValue;
                        break;
                    case "SubscribeTickets":
                        command.SubscribeTickets = (int)propertyValue;
                        break;
                    case "Favorability":
                        command.Favorability = (int)propertyValue;
                        break;
                    case "Deactivated":
                        command.Deactivated = (bool)propertyValue;
                        break;
                    default:
                        await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().WithContent($"Unknown property: {propertyName}").AsEphemeral(true));
                        return;
                }

                var result = await _updateService.UpdateAccountAsync(command);
                var response = UpdateAccountResponse.FromResult(result);

                if (response.Success)
                {
                    _botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Edit,
                        $"Updated {propertyName} for {ctx.User.Username} | {ctx.User.Id} to {propertyValue}");

                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Successfully updated {propertyName.ToLower()} to {propertyValue}.")
                            .AsEphemeral(true));
                }
                else
                {
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Failed to update {propertyName.ToLower()}: {response.Message}")
                            .AsEphemeral(true));
                }
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning,
                    $"Error updating {propertyName} for {ctx.User.Username} | {ctx.User.Id}: {ex.Message}");

                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"An error occurred while updating your account: {ex.Message}")
                        .AsEphemeral(true));
            }
        }
    }
} 