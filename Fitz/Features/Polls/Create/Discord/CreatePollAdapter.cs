using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Polls.Create.Domain;
using Fitz.Features.Polls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Create.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class CreatePollAdapter : ApplicationCommandModule
    {
        private readonly CreatePollService _createPollService;

        public CreatePollAdapter(CreatePollService createPollService)
        {
            _createPollService = createPollService ?? throw new ArgumentNullException(nameof(createPollService));
        }

        [SlashCommand("createpoll", "Create a new poll")]
        [RequireAccount]
        public async Task CreatePollCommand(
            InteractionContext ctx,
            [Option("title", "The title of the poll")] string title,
            [Option("description", "The description of the poll")] string description,
            [Option("option1", "First option")] string option1,
            [Option("option2", "Second option")] string option2,
            [Option("option3", "Third option (optional)")] string option3 = null,
            [Option("option4", "Fourth option (optional)")] string option4 = null,
            [Option("option5", "Fifth option (optional)")] string option5 = null,
            [Option("type", "The type of poll")] PollType pollType = PollType.Standard,
            [Option("duration", "Duration in hours (default: 24)")] long duration = 24,
            [Option("multiple_votes", "Allow multiple votes (default: false)")] bool allowMultipleVotes = false)
        {
            // Defer the response to give us time to process
            await ctx.DeferAsync();

            try
            {
                // Collect all non-null options
                var options = new List<string> { option1, option2 };
                if (!string.IsNullOrWhiteSpace(option3)) options.Add(option3);
                if (!string.IsNullOrWhiteSpace(option4)) options.Add(option4);
                if (!string.IsNullOrWhiteSpace(option5)) options.Add(option5);

                // Create the command
                var command = new CreatePollCommand(
                    accountId: ctx.User.Id,
                    channelId: ctx.Channel.Id,
                    title: title,
                    description: description,
                    options: options,
                    pollType: pollType,
                    endDate: DateTime.UtcNow.AddHours(duration),
                    allowMultipleVotes: allowMultipleVotes);

                // Execute the command
                var poll = await _createPollService.CreatePollAsync(command);

                // Create an embed for the response
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Poll Created")
                    .WithDescription($"Your poll '{poll.Title}' has been created successfully.")
                    .WithColor(DiscordColor.Green)
                    .AddField("ID", poll.Id.ToString(), true)
                    .AddField("End Date", poll.EndDate.ToString("g"), true)
                    .AddField("Options", string.Join("\n", poll.Options.Values.Select((opt, i) => $"{i + 1}. {opt}")))
                    .WithFooter($"Created by {ctx.User.Username}")
                    .WithTimestamp(DateTime.Now);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Error creating poll: {ex.Message}"));
            }
        }
    }
} 