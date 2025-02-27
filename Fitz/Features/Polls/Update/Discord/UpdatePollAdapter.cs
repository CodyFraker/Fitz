using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Polls.Models;
using Fitz.Features.Polls.Update.Domain;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Update.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class UpdatePollAdapter : ApplicationCommandModule
    {
        private readonly UpdatePollService _updatePollService;

        public UpdatePollAdapter(UpdatePollService updatePollService)
        {
            _updatePollService = updatePollService ?? throw new ArgumentNullException(nameof(updatePollService));
        }

        [SlashCommand("updatepoll", "Update a poll's status")]
        [RequireAccount]
        public async Task UpdatePollCommand(
            InteractionContext ctx,
            [Option("poll_id", "The ID of the poll to update")] long pollId,
            [Option("status", "The new status for the poll")] PollStatus status)
        {
            // Defer the response to give us time to process
            await ctx.DeferAsync();

            try
            {
                // Create the command
                var command = new UpdatePollCommand(
                    pollId: (int)pollId,
                    status: status,
                    userId: ctx.User.Id);

                // Execute the command
                var poll = await _updatePollService.UpdatePollStatusAsync(command);

                // Create an embed for the response
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Poll Updated")
                    .WithDescription($"Poll #{poll.Id} has been updated successfully.")
                    .WithColor(DiscordColor.Orange)
                    .AddField("Title", poll.Title, true)
                    .AddField("Status", poll.Status.ToString(), true)
                    .WithFooter($"Updated by {ctx.User.Username}")
                    .WithTimestamp(DateTime.Now);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Error updating poll: {ex.Message}"));
            }
        }
    }
} 