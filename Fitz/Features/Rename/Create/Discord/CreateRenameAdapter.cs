using System;
using System.Threading.Tasks;
using System.Linq;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Features.Rename.Create.Domain;

namespace Fitz.Features.Rename.Create.Discord
{
    public class CreateRenameAdapter
    {
        private readonly CreateRenameService _createRenameService;

        public CreateRenameAdapter(CreateRenameService createRenameService)
        {
            _createRenameService = createRenameService ?? throw new ArgumentNullException(nameof(createRenameService));
        }

        public async Task CreateRenameCommand(InteractionContext context)
        {
            await context.DeferAsync();

            try
            {
                var userOption = context.Interaction.Data.Options.FirstOrDefault(o => o.Name == "user");
                var newNameOption = context.Interaction.Data.Options.FirstOrDefault(o => o.Name == "new_name");
                var daysOption = context.Interaction.Data.Options.FirstOrDefault(o => o.Name == "days");
                
                var affectedUser = userOption?.Value as DiscordUser;
                var newName = newNameOption?.Value as string;
                var days = daysOption?.Value as int?;

                if (affectedUser == null)
                {
                    await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("User not found.").AsEphemeral(true));
                    return;
                }

                if (string.IsNullOrWhiteSpace(newName))
                {
                    await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("New name cannot be empty.").AsEphemeral(true));
                    return;
                }

                // Calculate cost based on days
                int cost = CalculateRenameCost(days);

                // Create the command
                var createRenameCommand = new CreateRenameCommand(
                    affectedUser.Id,
                    context.User.Id,
                    affectedUser.Username,
                    newName,
                    days,
                    cost
                );

                // Create the rename
                var rename = await _createRenameService.CreateRenameAsync(createRenameCommand);

                // Create response embed
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Rename Request Created")
                    .WithDescription($"Rename request for {affectedUser.Mention} has been created.")
                    .WithColor(DiscordColor.Green)
                    .AddField("From", affectedUser.Username, true)
                    .AddField("To", newName, true)
                    .AddField("Cost", $"{cost} coins", true);

                if (days.HasValue)
                {
                    embed.AddField("Duration", $"{days.Value} days", true);
                    embed.AddField("Status", "Pending Approval", true);
                }
                else
                {
                    embed.AddField("Duration", "Permanent", true);
                    embed.AddField("Status", "Permanent", true);
                }

                embed.WithFooter($"Rename ID: {rename.Id}");

                await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                await context.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Error creating rename: {ex.Message}").AsEphemeral(true));
            }
        }

        private int CalculateRenameCost(int? days)
        {
            // Base cost for any rename
            const int BaseCost = 1000;
            
            if (!days.HasValue)
            {
                // Permanent renames are more expensive
                return BaseCost * 10;
            }
            
            // Cost scales with duration
            return BaseCost + (days.Value * 100);
        }
    }
} 