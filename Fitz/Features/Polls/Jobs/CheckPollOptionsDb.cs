using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Jobs
{
    public class CheckPollOptionsDb(DiscordClient dClient, PollService pollService, BotLog botLog)
    {
        private readonly DiscordClient dClient = dClient;
        private readonly PollService PollService = pollService;
        private readonly BotLog botLog = botLog;

        public async Task Execute(DiscordMessage message, Poll poll)
        {
            this.botLog.Information(LogConsoleSettings.Jobs, PollEmojis.InfoIcon, $"Checking if poll contains all valid poll options...");

            // Retrieve all poll options from the database for this poll.
            List<PollOptions> pollOptions = this.PollService.GetPollOptions(poll);

            foreach (DiscordReaction reaction in message.Reactions)
            {
                // If there is a reaction that is not a valid poll option, remove it.
                if (!pollOptions.Any(x => x.EmojiName.Contains(reaction.Emoji.Name)))
                {
                    this.botLog.Information(LogConsoleSettings.Jobs, PollEmojis.InfoIcon, $"Found non-valid poll option on poll #{poll.Id}");
                    await message.DeleteReactionsEmojiAsync(reaction.Emoji);
                }
            }

            // Check to see if all poll options were added to the message
            foreach (PollOptions option in pollOptions)
            {
                if (!message.Reactions.Any(x => x.Emoji.Name.Contains(option.EmojiName)))
                {
                    if (option.EmojiName.Contains(':'))
                    {
                        await message.CreateReactionAsync(DiscordEmoji.FromName(this.dClient, option.EmojiName));
                    }
                    else
                    {
                        await message.CreateReactionAsync(DiscordEmoji.FromName(this.dClient, $":{option.EmojiName}:"));
                    }
                }
            }
        }
    }
}