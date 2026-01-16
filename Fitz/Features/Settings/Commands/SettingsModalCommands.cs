using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;
using Fitz.Features.Settings.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Fitz.Features.Settings.Commands
{
    public class SettingsModalCommands(IServiceScopeFactory scopeFactory) : ModalCommandModule
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        [ModalCommand("LotteryDuration")]
        public async Task SetLotteryDuration(ModalContext ctx, int lotteryDuration)
        {
            if (lotteryDuration < 0 || lotteryDuration > 365)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Lottery Duration must be between 1 and 365 days.")
                    .AsEphemeral(true));
                return;
            }

            var command = new SetLotteryDurationCommand(scopeFactory);
            var settingsResult = await command.ExecuteAsync(lotteryDuration);
            
            if (settingsResult.Success)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Lottery Duration has been updated.")
                        .AsEphemeral(true));
                return;
            }
            else
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("An error occurred while updating the Lottery Duration.")
                        .AsEphemeral(true));
                return;
            }
        }

        [ModalCommand("MaxTickets")]
        public async Task SetMaxTickets(ModalContext ctx, int maxTickets)
        {
            if (maxTickets <= 0 || maxTickets > 999)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("You can only set the max amount of lottery tickets per user between 1 and 999.")
                    .AsEphemeral(true));
                return;
            }

            var command = new SetMaxTicketsCommand(scopeFactory);
            var settingsResult = await command.ExecuteAsync(maxTickets);
            
            if (settingsResult.Success)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Max tickets for each user was set to {maxTickets}")
                        .AsEphemeral(true));
                return;
            }
            else
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("An error occurred while updating the Lottery Duration.")
                        .AsEphemeral(true));
                return;
            }
        }

        [ModalCommand("LotteryPool")]
        public async Task SetLotteryPool(ModalContext ctx, int value)
        {
            if (value <= 0)
            {
                return;
            }

            var command = new SetBaseLotteryPoolCommand(scopeFactory);
            await command.ExecuteAsync(value);
        }

        [ModalCommand("LotteryPoolRollover")]
        public async Task SetLotteryPoolRollover(ModalContext ctx, bool value)
        {
            var command = new SetLotteryPoolRolloverCommand(scopeFactory);
            await command.ExecuteAsync(value);
        }

        [ModalCommand("TicketCost")]
        public async Task SetTicketCost(ModalContext ctx, int value)
        {
            if (value <= 0)
            {
                return;
            }

            var command = new SetTicketCostCommand(scopeFactory);
            await command.ExecuteAsync(value);
        }

        [ModalCommand("BaseHappyHourAmount")]
        public async Task SetBaseHappyHourAmount(ModalContext ctx, int value)
        {
            if (value <= 0)
            {
                return;
            }

            var command = new SetHappyHourBaseAmountCommand(scopeFactory);
            await command.ExecuteAsync(value);
        }

        [ModalCommand("AccountCreationBonusAmount")]
        public async Task SetAccountCreationBonusAmount(ModalContext ctx, int value)
        {
            if (value <= 0)
            {
                return;
            }

            var command = new SetAccountCreationBonusAmountCommand(scopeFactory);
            await command.ExecuteAsync(value);
        }

        [ModalCommand("RenameBaseCost")]
        public async Task SetRenameBaseCost(ModalContext ctx, int value)
        {
            if (value <= 0)
            {
                return;
            }

            var command = new SetRenameBaseCostCommand(scopeFactory);
            await command.ExecuteAsync(value);
        }
    }
}
