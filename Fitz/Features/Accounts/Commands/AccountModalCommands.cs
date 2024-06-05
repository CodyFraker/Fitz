using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Commands
{
    public class AccountModalCommands(AccountService accountService, DiscordClient dClient) : ModalCommandModule
    {
        private readonly DiscordClient dClient = dClient;
        private readonly AccountService accountService = accountService;

        [ModalCommand("set_safe_balance")]
        public async Task CreateAccountAsync(ModalContext ctx, int value)
        {
            // Update user account settings and set safe balance to the value
            if (value <= 0)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Value cannot be less than or equal to 0.")
                    .AsEphemeral(true));
                return;
            }
            else
            {
                //await this.accountService.SetSafeBalance(ctx.User.Id, value);
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent(""));
                return;
            }
        }

        [ModalCommand("set_ticket_amount")]
        public async Task SetTicketAmount(ModalContext ctx, int value)
        {
            // Update user account settings and set safe balance to the value
            if (value <= 0)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Value cannot be less than or equal to 0.")
                    .AsEphemeral(true));
                return;
            }
            else
            {
                //await this.accountService.SetSafeBalance(ctx.User.Id, value);
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent(""));
                return;
            }
        }
    }
}