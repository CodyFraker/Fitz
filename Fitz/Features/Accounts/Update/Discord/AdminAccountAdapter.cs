using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Features.Accounts.Update.Domain;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Fitz.Features.Accounts.Update.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    [SlashCommandGroup("admin-account", "Admin commands for account management")]
    public class AdminAccountAdapter : ApplicationCommandModule
    {
        private readonly UpdateAccountService _updateService;
        private readonly BotLog _botLog;

        public AdminAccountAdapter(UpdateAccountService updateService, BotLog botLog)
        {
            _updateService = updateService;
            _botLog = botLog;
        }

        [SlashCommand("set-favorability", "Set a user's favorability")]
        public async Task SetFavorability(InteractionContext ctx, 
            [Option("user", "The user to update")] DiscordUser user,
            [Option("favorability", "The new favorability value (0-100)")] long favorability)
        {
            // Check if the command user has admin permissions
            if (!HasAdminPermission(ctx))
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("You don't have permission to use this command.").AsEphemeral(true));
                return;
            }

            if (favorability < 0 || favorability > 100)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("Favorability must be between 0 and 100.").AsEphemeral(true));
                return;
            }

            await UpdateUserProperty(ctx, user.Id, "Favorability", (int)favorability);
        }

        [SlashCommand("set-deactivated", "Activate or deactivate a user's account")]
        public async Task SetDeactivated(InteractionContext ctx,
            [Option("user", "The user to update")] DiscordUser user,
            [Option("deactivated", "Whether the account should be deactivated")] bool deactivated)
        {
            // Check if the command user has admin permissions
            if (!HasAdminPermission(ctx))
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("You don't have permission to use this command.").AsEphemeral(true));
                return;
            }

            await UpdateUserProperty(ctx, user.Id, "Deactivated", deactivated);
        }

        private bool HasAdminPermission(InteractionContext ctx)
        {
            // Check if the user has a role with Administrator permission or is a server owner
            return ctx.Member.IsOwner || ctx.Member.Roles.Any(r => r.Name.Contains("Admin"));
        }

        private async Task UpdateUserProperty(InteractionContext ctx, ulong userId, string propertyName, object propertyValue)
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
                        $"Admin {ctx.User.Username} updated {propertyName} for user {userId} to {propertyValue}");

                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Successfully updated {propertyName.ToLower()} for user {userId} to {propertyValue}.")
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
                    $"Error updating {propertyName} for user {userId}: {ex.Message}");

                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"An error occurred while updating the account: {ex.Message}")
                        .AsEphemeral(true));
            }
        }
    }
} 