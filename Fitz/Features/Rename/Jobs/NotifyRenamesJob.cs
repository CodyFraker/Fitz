using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Rename.Common;
using Fitz.Features.Rename.Notify.Domain;
using Fitz.Variables.Emojis;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Fitz.Features.Rename.Jobs
{
    public class NotifyRenamesJob : ITimedJob
    {
        private readonly DiscordClient _dClient;
        private readonly BotLog _botLog;
        private readonly NotifyRenameService _notifyRenameService;

        public NotifyRenamesJob(DiscordClient dClient, NotifyRenameService notifyRenameService, BotLog botLog)
        {
            _dClient = dClient ?? throw new ArgumentNullException(nameof(dClient));
            _notifyRenameService = notifyRenameService ?? throw new ArgumentNullException(nameof(notifyRenameService));
            _botLog = botLog ?? throw new ArgumentNullException(nameof(botLog));
        }

        public ulong Emoji => ManageRoleEmojis.Warning;

        public int Interval => 15; // Run every 15 minutes

        public async Task Execute()
        {
            try
            {
                _botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Checking for unnotified renames...");

                // Get all unnotified renames
                var unnotifiedRenames = await _notifyRenameService.GetUnnotifiedRenamesAsync();

                if (unnotifiedRenames.Length == 0)
                {
                    _botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "No unnotified renames found.");
                    return;
                }

                _botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Found {unnotifiedRenames.Length} unnotified renames.");

                // Process each unnotified rename
                foreach (var rename in unnotifiedRenames)
                {
                    try
                    {
                        // Get the user
                        var user = await _dClient.GetUserAsync(rename.AffectedUserId);
                        if (user == null)
                        {
                            _botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"User {rename.AffectedUserId} not found for rename ID {rename.Id}.");
                            continue;
                        }

                        // Create and send the notification
                        var embed = CreateNotificationEmbed(rename);
                        
                        // Get the guilds the bot is in
                        var guilds = _dClient.Guilds;
                        bool notified = false;
                        
                        // Try to find the user in one of the guilds
                        foreach (var guildPair in guilds)
                        {
                            try
                            {
                                var guild = guildPair.Value;
                                var member = await guild.GetMemberAsync(user.Id);
                                
                                if (member != null)
                                {
                                    var dmChannel = await member.CreateDmChannelAsync();
                                    await dmChannel.SendMessageAsync(embed: embed);
                                    notified = true;
                                    break;
                                }
                            }
                            catch
                            {
                                // Continue to the next guild if there's an error
                                continue;
                            }
                        }
                        
                        if (!notified)
                        {
                            _botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Could not find user {user.Username} (ID: {rename.AffectedUserId}) in any guild for rename ID {rename.Id}.");
                            continue;
                        }

                        // Mark the rename as notified
                        await _notifyRenameService.NotifyRenameAsync(new NotifyRenameCommand(rename.Id));

                        _botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Notified user {user.Username} (ID: {rename.AffectedUserId}) about rename ID {rename.Id}.");
                    }
                    catch (Exception ex)
                    {
                        _botLog.Error(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Failed to notify user {rename.AffectedUserId} about rename ID {rename.Id}: {ex.Message}");
                    }
                }

                _botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Finished processing unnotified renames.");
            }
            catch (Exception ex)
            {
                _botLog.Error(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Error in NotifyRenamesJob: {ex.Message}");
            }
        }

        private DiscordEmbed CreateNotificationEmbed(Common.Rename rename)
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
    }
}