using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Features.Rename.Common;
using Fitz.Features.Rename.Update.Domain;

namespace Fitz.Features.Rename.Update.Discord
{
    public class UpdateRenameAdapter
    {
        private readonly UpdateRenameService _updateRenameService;

        public UpdateRenameAdapter(UpdateRenameService updateRenameService)
        {
            _updateRenameService = updateRenameService ?? throw new ArgumentNullException(nameof(updateRenameService));
        }

        public async Task UpdateRenameCommand(InteractionContext context)
        {
            await context.DeferAsync();

            try
            {
                var renameIdOption = context.Interaction.Data.Options.FirstOrDefault(o => o.Name == "rename_id");
                var statusOption = context.Interaction.Data.Options.FirstOrDefault(o => o.Name == "status");

                if (renameIdOption?.Value is not int renameId)
                {
                    await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Invalid rename ID.").AsEphemeral(true));
                    return;
                }

                if (statusOption?.Value is not string statusStr || !Enum.TryParse<RenameStatus>(statusStr, true, out var newStatus))
                {
                    await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Invalid status. Valid statuses are: Active, Expired, BoughtOut, Permanent").AsEphemeral(true));
                    return;
                }

                // Create the command
                var updateRenameCommand = new UpdateRenameCommand(renameId, newStatus);

                // Update the rename
                var rename = await _updateRenameService.UpdateRenameStatusAsync(updateRenameCommand);

                // Create response embed
                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Rename #{rename.Id} Updated")
                    .WithDescription($"Rename status has been updated to **{rename.Status}**.")
                    .WithColor(DiscordColor.Blue)
                    .AddField("User", $"<@{rename.AffectedUserId}>", true)
                    .AddField("From", rename.OldName, true)
                    .AddField("To", rename.NewName, true);

                if (rename.StartDate.HasValue)
                {
                    embed.AddField("Start Date", $"{rename.StartDate:yyyy-MM-dd}", true);
                }

                if (rename.Expiration.HasValue)
                {
                    var daysLeft = (rename.Expiration.Value - DateTime.UtcNow).Days;
                    embed.AddField("Expiration", $"{rename.Expiration:yyyy-MM-dd} ({daysLeft} days left)", true);
                }

                await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Error updating rename: {ex.Message}").AsEphemeral(true));
            }
        }
    }
} 