using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;
using Fitz.Core.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Core.Commands.Settings
{
    public class SettingsModalComands(SettingsService settingsService) : ModalCommandModule
    {
        private readonly SettingsService settingsService = settingsService;

        #region Set Lottery Duration Modal

        [ModalCommand("LotteryDuration")]
        public async Task SetLotteryDuration(ModalContext ctx, int lotteryDuration)
        {
            // Update user account settings and set safe balance to the value
            if (lotteryDuration < 0 || lotteryDuration > 365)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Lottery Duration must be between 1 and 365 days.")
                    .AsEphemeral(true));
                return;
            }
            else
            {
                var settingsResult = await settingsService.SetLotteryDuration(lotteryDuration);
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
        }

        #endregion Set Lottery Duration Modal

        #region Set Max Tickets

        [ModalCommand("MaxTickets")]
        public async Task SetMaxTickets(ModalContext ctx, int maxTickets)
        {
            // Update user account settings and set safe balance to the value
            if (maxTickets <= 0 || maxTickets > 999)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("You can only set the max amount of lottery tickets per user between 1 and 999.")
                    .AsEphemeral(true));
                return;
            }
            else
            {
                var settingsResult = await settingsService.SetMaxTickets(maxTickets);
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
        }

        #endregion Set Max Tickets

        #region Set Lottery Pool Modal

        [ModalCommand("LotteryPool")]
        public async Task SetLotteryPool(ModalContext ctx, int value)
        {
            if (value <= 0)
            {
                return;
            }
            else
            {
                return;
            }
        }

        #endregion Set Lottery Pool Modal

        #region Set Lottery Pool Rollover Modal

        [ModalCommand("LotteryPoolRollover")]
        public async Task SetLotteryPoolRollover(ModalContext ctx, bool value)
        {
            // Update user account settings and set safe balance to the value
            if (value)
            {
                return;
            }
            else
            {
                return;
            }
        }

        #endregion Set Lottery Pool Rollover Modal

        #region Set Ticket Cost Modal

        [ModalCommand("TicketCost")]
        public async Task SetTicketCost(ModalContext ctx, int value)
        {
            // Update user account settings and set safe balance to the value
            if (value <= 0)
            {
                return;
            }
            else
            {
                return;
            }
        }

        #endregion Set Ticket Cost Modal

        #region Set Base Happy Hour Amount Modal

        [ModalCommand("BaseHappyHourAmount")]
        public async Task SetBaseHappyHourAmount(ModalContext ctx, int value)
        {
            // Update user account settings and set safe balance to the value
            if (value <= 0)
            {
                return;
            }
            else
            {
                return;
            }
        }

        #endregion Set Base Happy Hour Amount Modal

        #region Set Account Creation Bonus Amount Modal

        [ModalCommand("AccountCreationBonusAmount")]
        public async Task SetAccountCreationBonusAmount(ModalContext ctx, int value)
        {
            // Update user account settings and set safe balance to the value
            if (value <= 0)
            {
                return;
            }
            else
            {
                return;
            }
        }

        #endregion Set Account Creation Bonus Amount Modal

        #region Set Rename Base Cost Modal

        [ModalCommand("RenameBaseCost")]
        public async Task SetRenameBaseCost(ModalContext ctx, int value)
        {
            // Update user account settings and set safe balance to the value
            if (value <= 0)
            {
                return;
            }
            else
            {
                return;
            }
        }

        #endregion Set Rename Base Cost Modal
    }
}