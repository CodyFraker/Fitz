using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Features.Rename.Common;
using Fitz.Features.Rename.Get.Domain;

namespace Fitz.Features.Rename.Get.Discord
{
    public class GetRenameAdapter
    {
        private readonly GetRenameService _getRenameService;

        public GetRenameAdapter(GetRenameService getRenameService)
        {
            _getRenameService = getRenameService ?? throw new ArgumentNullException(nameof(getRenameService));
        }

        public async Task GetRenameCommand(InteractionContext context)
        {
            await context.DeferAsync();

            try
            {
                var renameIdOption = context.Interaction.Data.Options.FirstOrDefault(o => o.Name == "rename_id");
                var userOption = context.Interaction.Data.Options.FirstOrDefault(o => o.Name == "user");

                int? renameId = renameIdOption?.Value as int?;
                ulong? userId = userOption?.Value as DiscordUser != null 
                    ? ((DiscordUser)userOption.Value).Id 
                    : null;

                var getRenameCommand = new GetRenameCommand(renameId, userId);
                var rename = await _getRenameService.GetRenameByIdAsync(getRenameCommand);

                if (rename != null)
                {
                    var embed = CreateRenameEmbed(rename);
                    await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
                }
                else
                {
                    await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("No rename found with the specified criteria.").AsEphemeral(true));
                }
            }
            catch (Exception ex)
            {
                await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Error retrieving rename: {ex.Message}").AsEphemeral(true));
            }
        }

        public async Task GetRenameHistoryCommand(InteractionContext context)
        {
            await context.DeferAsync();

            try
            {
                var userOption = context.Interaction.Data.Options.FirstOrDefault(o => o.Name == "user");
                var user = userOption?.Value as DiscordUser;

                if (user == null)
                {
                    await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("User not found.").AsEphemeral(true));
                    return;
                }

                var renames = await _getRenameService.GetRenameHistoryByUserIdAsync(user.Id);
                
                if (renames != null && renames.Any())
                {
                    var embed = new DiscordEmbedBuilder()
                        .WithTitle($"Rename History for {user.Username}")
                        .WithColor(DiscordColor.Blue)
                        .WithFooter($"Total renames: {renames.Count()}")
                        .WithTimestamp(DateTime.Now);

                    foreach (var rename in renames.Take(10)) // Limit to 10 to avoid embed limits
                    {
                        embed.AddField(
                            $"ID: {rename.Id} - {rename.Status}",
                            $"From: {rename.OldName}\n" +
                            $"To: {rename.NewName}\n" +
                            $"Requested by: <@{rename.RequestedUserId}>\n" +
                            $"Created: {rename.Timestamp:yyyy-MM-dd}\n" +
                            (rename.Expiration.HasValue ? $"Expires: {rename.Expiration:yyyy-MM-dd}" : "No expiration")
                        );
                    }

                    await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
                }
                else
                {
                    await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"No rename history found for {user.Username}.").AsEphemeral(true));
                }
            }
            catch (Exception ex)
            {
                await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Error retrieving rename history: {ex.Message}").AsEphemeral(true));
            }
        }

        private DiscordEmbed CreateRenameEmbed(Common.Rename rename)
        {
            var statusEmoji = GetStatusEmoji(rename.Status);
            var statusColor = GetStatusColor(rename.Status);

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"Rename #{rename.Id} {statusEmoji}")
                .WithDescription($"**From:** {rename.OldName}\n**To:** {rename.NewName}")
                .WithColor(statusColor)
                .AddField("Affected User", $"<@{rename.AffectedUserId}>", true)
                .AddField("Requested By", $"<@{rename.RequestedUserId}>", true)
                .AddField("Status", rename.Status.ToString(), true)
                .AddField("Cost", $"{rename.Cost} coins", true);

            if (rename.Days.HasValue)
            {
                embed.AddField("Duration", $"{rename.Days} days", true);
            }

            if (rename.StartDate.HasValue)
            {
                embed.AddField("Start Date", $"{rename.StartDate:yyyy-MM-dd}", true);
            }

            if (rename.Expiration.HasValue)
            {
                var daysLeft = (rename.Expiration.Value - DateTime.UtcNow).Days;
                embed.AddField("Expiration", $"{rename.Expiration:yyyy-MM-dd} ({daysLeft} days left)", true);
            }

            embed.WithFooter($"Created on {rename.Timestamp:yyyy-MM-dd}");
            
            return embed.Build();
        }

        private string GetStatusEmoji(RenameStatus status)
        {
            return status switch
            {
                RenameStatus.Pending => "⏳",
                RenameStatus.Active => "✅",
                RenameStatus.Expired => "⌛",
                RenameStatus.BoughtOut => "💰",
                RenameStatus.Permanent => "🔒",
                _ => "❓"
            };
        }

        private DiscordColor GetStatusColor(RenameStatus status)
        {
            return status switch
            {
                RenameStatus.Pending => DiscordColor.Orange,
                RenameStatus.Active => DiscordColor.Green,
                RenameStatus.Expired => DiscordColor.LightGray,
                RenameStatus.BoughtOut => DiscordColor.Gold,
                RenameStatus.Permanent => DiscordColor.Purple,
                _ => DiscordColor.DarkGray
            };
        }
    }
} 