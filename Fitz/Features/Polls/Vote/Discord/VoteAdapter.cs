using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Polls.Vote.Domain;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Vote.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class VoteAdapter : ApplicationCommandModule
    {
        private readonly VoteService _voteService;

        public VoteAdapter(VoteService voteService)
        {
            _voteService = voteService ?? throw new ArgumentNullException(nameof(voteService));
        }

        [SlashCommand("vote", "Vote on a poll")]
        [RequireAccount]
        public async Task VoteCommand(
            InteractionContext ctx,
            [Option("poll_id", "The ID of the poll to vote on")] long pollId,
            [Option("option", "The option number to vote for (1-based)")] long optionNumber)
        {
            // Defer the response to give us time to process
            await ctx.DeferAsync();

            try
            {
                // Convert the 1-based option number to a 0-based index
                int optionIndex = (int)optionNumber - 1;
                
                if (optionIndex < 0)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("Option number must be at least 1."));
                    return;
                }

                // Create the command
                var command = new VoteCommand(
                    pollId: (int)pollId,
                    userId: ctx.User.Id,
                    optionIndex: optionIndex);

                // Execute the command
                bool success = await _voteService.VoteAsync(command);

                if (success)
                {
                    // Create an embed for the response
                    var embed = new DiscordEmbedBuilder()
                        .WithTitle("Vote Recorded")
                        .WithDescription($"Your vote for poll #{pollId} has been recorded successfully.")
                        .WithColor(DiscordColor.Green)
                        .WithFooter($"Voted by {ctx.User.Username}")
                        .WithTimestamp(DateTime.Now);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Error voting on poll: {ex.Message}"));
            }
        }
    }
} 