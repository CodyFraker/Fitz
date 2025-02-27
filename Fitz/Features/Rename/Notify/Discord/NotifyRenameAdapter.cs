using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Features.Rename.Common;
using Fitz.Features.Rename.Notify.Domain;

namespace Fitz.Features.Rename.Notify.Discord
{
    public class NotifyRenameAdapter
    {
        private readonly NotifyRenameService _notifyRenameService;
        private readonly DiscordClient _discordClient;

        public NotifyRenameAdapter(NotifyRenameService notifyRenameService, DiscordClient discordClient)
        {
            _notifyRenameService = notifyRenameService ?? throw new ArgumentNullException(nameof(notifyRenameService));
            _discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
        }

        public async Task NotifyRenameCommand(InteractionContext context)
        {
            await context.DeferAsync();

            try
            {
                var renameIdOption = context.Interaction.Data.Options.FirstOrDefault(o => o.Name == "rename_id");
                
                if (renameIdOption?.Value is not int renameId)
                {
                    await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Invalid rename ID.").AsEphemeral(true));
                    return;
                }

                // Create the command
                var notifyRenameCommand = new NotifyRenameCommand(renameId);

                // Notify about the rename
                var rename = await _notifyRenameService.NotifyRenameAsync(notifyRenameCommand);

                // Create response embed
                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Rename #{rename.Id} Notification")
                    .WithDescription($"User <@{rename.AffectedUserId}> has been notified about their rename.")
                    .WithColor(DiscordColor.Green)
                    .AddField("From", rename.OldName, true)
                    .AddField("To", rename.NewName, true)
                    .AddField("Status", rename.Status.ToString(), true);

                await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));

                // Send a direct message to the affected user
                try
                {
                    var guild = context.Guild;
                    var member = await guild.GetMemberAsync(rename.AffectedUserId);
                    if (member != null)
                    {
                        var userEmbed = CreateUserNotificationEmbed(rename);
                        await member.SendMessageAsync(userEmbed);
                    }
                }
                catch (Exception ex)
                {
                    await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Failed to send DM to user: {ex.Message}").AsEphemeral(true));
                }
            }
            catch (Exception ex)
            {
                await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Error notifying about rename: {ex.Message}").AsEphemeral(true));
            }
        }

        private DiscordEmbed CreateUserNotificationEmbed(Common.Rename rename)
        {
            string message = rename.Status switch
            {
                RenameStatus.Active => $"Your nickname has been changed to **{rename.NewName}**.",
                RenameStatus.Expired => $"Your nickname **{rename.NewName}** has expired and has been reset to your original name.",
                RenameStatus.BoughtOut => $"Your nickname **{rename.NewName}** has been bought out and has been reset to your original name.",
                _ => $"Your nickname status has been updated to **{rename.Status}**."
            };

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Nickname Update")
                .WithDescription(message)
                .WithColor(DiscordColor.Blue);

            if (rename.Status == RenameStatus.Active && rename.Expiration.HasValue)
            {
                var daysLeft = (rename.Expiration.Value - DateTime.UtcNow).Days;
                embed.AddField("Expiration", $"{rename.Expiration:yyyy-MM-dd} ({daysLeft} days left)", true);
            }

            return embed.Build();
        }

        public async Task SendBulkNotifications()
        {
            var unnotifiedRenames = await _notifyRenameService.GetUnnotifiedRenamesAsync();
            
            foreach (var rename in unnotifiedRenames)
            {
                try
                {
                    // For bulk notifications, we need to find a guild where the user is a member
                    var guild = _discordClient.Guilds.Values.FirstOrDefault();
                    if (guild != null)
                    {
                        var member = await guild.GetMemberAsync(rename.AffectedUserId);
                        if (member != null)
                        {
                            var userEmbed = CreateUserNotificationEmbed(rename);
                            await member.SendMessageAsync(userEmbed);
                            
                            // Mark as notified
                            await _notifyRenameService.NotifyRenameAsync(new NotifyRenameCommand(rename.Id));
                        }
                    }
                }
                catch (Exception)
                {
                    // Log error but continue with other notifications
                    continue;
                }
            }
        }
    }
} 