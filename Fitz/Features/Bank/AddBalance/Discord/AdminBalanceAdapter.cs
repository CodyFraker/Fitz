using System;
using System.Linq;
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
    [SlashCommandGroup("admin-bank", "Admin commands for bank management")]
    public class AdminBalanceAdapter : ApplicationCommandModule
    {
        private readonly AddBalanceService _balanceService;
        private readonly BotLog _botLog;

        public AdminBalanceAdapter(AddBalanceService balanceService, BotLog botLog)
        {
            _balanceService = balanceService ?? throw new ArgumentNullException(nameof(balanceService));
            _botLog = botLog ?? throw new ArgumentNullException(nameof(botLog));
        }

        [SlashCommand("add-beer", "Add beer to a user's account")]
        public async Task AddBeerCommand(
            InteractionContext ctx,
            [Option("user", "The user to add beer to")] DiscordUser user,
            [Option("amount", "The amount of beer to add")] long amount)
        {
            try
            {
                // Check if the command user has admin permissions
                if (!HasAdminPermission(ctx))
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("You don't have permission to use this command.")
                            .AsEphemeral(true));
                    return;
                }

                if (amount <= 0)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Amount must be greater than 0.")
                            .AsEphemeral(true));
                    return;
                }

                var command = new AddBalanceCommand(
                    recipientId: user.Id,
                    senderId: ctx.User.Id,
                    amount: (int)amount,
                    reason: TransactionReason.AdminAddBalance,
                    updateLifetimeBalance: true);

                var result = await _balanceService.AddBalanceAsync(command);
                var response = AddBalanceResponse.FromResult(result);

                if (response.Success)
                {
                    _botLog.Information(LogConsoleSettings.BankLog, BankEmojis.Add,
                        $"Admin {ctx.User.Username} added {amount} beer to {user.Username}");

                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Added {amount} beer to {user.Username}.")
                            .AsEphemeral(true));
                }
                else
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Failed to add beer: {response.Message}")
                            .AsEphemeral(true));
                }
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.BankLog, BankEmojis.Warning,
                    $"Error adding beer: {ex.Message}");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"An error occurred: {ex.Message}")
                        .AsEphemeral(true));
            }
        }

        [SlashCommand("remove-beer", "Remove beer from a user's account")]
        public async Task RemoveBeerCommand(
            InteractionContext ctx,
            [Option("user", "The user to remove beer from")] DiscordUser user,
            [Option("amount", "The amount of beer to remove")] long amount)
        {
            try
            {
                // Check if the command user has admin permissions
                if (!HasAdminPermission(ctx))
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("You don't have permission to use this command.")
                            .AsEphemeral(true));
                    return;
                }

                if (amount <= 0)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Amount must be greater than 0.")
                            .AsEphemeral(true));
                    return;
                }

                var command = new DeductBalanceCommand(
                    userId: user.Id,
                    amount: (int)amount,
                    reason: TransactionReason.AdminRemoveBalance);

                var result = await _balanceService.DeductBalanceAsync(command);
                var response = AddBalanceResponse.FromResult(result);

                if (response.Success)
                {
                    _botLog.Information(LogConsoleSettings.BankLog, BankEmojis.Remove,
                        $"Admin {ctx.User.Username} removed {amount} beer from {user.Username}");

                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Removed {amount} beer from {user.Username}.")
                            .AsEphemeral(true));
                }
                else
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Failed to remove beer: {response.Message}")
                            .AsEphemeral(true));
                }
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.BankLog, BankEmojis.Warning,
                    $"Error removing beer: {ex.Message}");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"An error occurred: {ex.Message}")
                        .AsEphemeral(true));
            }
        }

        private bool HasAdminPermission(InteractionContext ctx)
        {
            // Check if the user has a role with Administrator permission or is a server owner
            return ctx.Member.IsOwner || ctx.Member.Roles.Any(r => r.Name.Contains("Admin"));
        }
    }
} 