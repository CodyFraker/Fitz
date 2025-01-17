using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Polls
{
    public class PollJob(DiscordClient dClient, PollService pollService, BotLog botLog) : ITimedJob
    {
        private readonly DiscordClient dClient = dClient;
        private readonly PollService PollService = pollService;
        private readonly BotLog botLog = botLog;

        public ulong Emoji => PollEmojis.InfoIcon;

        public int Interval => 5;

        public async Task Execute()
        {
            this.botLog.Information(LogConsoleSettings.Jobs, PollEmojis.InfoIcon, $"Starting Poll Job...");
            await ProcessPollChannel();
            this.botLog.Information(LogConsoleSettings.Jobs, PollEmojis.InfoIcon, $"Finished Poll Job");
        }

        private async Task ProcessPollChannel()
        {
            DiscordChannel pollChannel = await dClient.GetChannelAsync(Variables.Channels.Waterbear.Polls);
            IAsyncEnumerable<DiscordMessage> pollChannelMessages = pollChannel.GetMessagesAsync();

            await foreach (DiscordMessage message in pollChannelMessages)
            {
                await ProcessPollMessage(message);
            }
        }

        private async Task ProcessPollMessage(DiscordMessage message)
        {
            Poll poll = this.PollService.GetPoll(message.Id);

            if (poll == null)
            {
                await DeleteInvalidPollMessage(message);
                return;
            }

            List<PollOptions> pollOptions = this.PollService.GetPollOptions(poll);
            List<DiscordReaction> reactions = message.Reactions.ToList();

            await AddMissingPollOptions(message, pollOptions, reactions);
            await RemoveInvalidReactions(poll, message, pollOptions, reactions);
            await ProcessUserVotes(message, poll, pollOptions);
        }

        private async Task DeleteInvalidPollMessage(DiscordMessage message)
        {
            await message.DeleteAsync("Deleting message from poll channel. Message was not a valid poll.");
            // TODO: Determine if the message is supposed to be a poll that wasn't saved in the database.
        }

        private async Task AddMissingPollOptions(DiscordMessage message, List<PollOptions> pollOptions, List<DiscordReaction> reactions)
        {
            if (pollOptions.Count != reactions.Count)
            {
                foreach (PollOptions option in pollOptions)
                {
                    if (!reactions.Any(x => x.Emoji.Name.Contains(option.EmojiName)))
                    {
                        if (option.EmojiId != 0 && option.EmojiId != null)
                        {
                            await message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(dClient, option.EmojiId.Value));
                        }
                        else
                        {
                            await message.CreateReactionAsync(DiscordEmoji.FromName(dClient, $":{option.EmojiName}:"));
                        }
                    }
                }
            }
        }

        private async Task RemoveInvalidReactions(Poll poll, DiscordMessage message, List<PollOptions> pollOptions, List<DiscordReaction> reactions)
        {
            foreach (DiscordReaction pollReaction in message.Reactions)
            {
                if (!pollOptions.Any(x => x.EmojiName.Contains(pollReaction.Emoji.Name)))
                {
                    //await message.DeleteReactionsEmojiAsync(pollReaction.Emoji);
                }
                else
                {
                    foreach (DiscordUser user in await message.GetReactionsAsync(pollReaction.Emoji))
                    {
                        if (user == null)
                        {
                            return;
                        }

                        if (user.IsBot)
                        {
                            continue;
                        }

                        Vote userVote = this.PollService.GetVoteByUserOnPoll(poll, user.Id);

                        if (userVote == null)
                        {
                            await this.PollService.AddVote(poll, pollOptions.Where(x => x.EmojiName == pollReaction.Emoji.Name).FirstOrDefault(), user.Id);
                        }
                        else
                        {
                            if (userVote.PollId != poll.Id)
                            {
                                //await message.DeleteReactionsEmojiAsync(pollReaction.Emoji);
                            }

                            if (!pollOptions.Any(x => x.EmojiName == pollReaction.Emoji.Name))
                            {
                                //await message.DeleteReactionsEmojiAsync(pollReaction.Emoji);
                            }
                        }
                    }
                }
            }
        }

        private async Task ProcessUserVotes(DiscordMessage message, Poll poll, List<PollOptions> pollOptions)
        {
            foreach (DiscordReaction pollReaction in message.Reactions)
            {
                if (!pollOptions.Any(x => x.EmojiName.Contains(pollReaction.Emoji.Name)))
                {
                    //await message.DeleteReactionsEmojiAsync(pollReaction.Emoji);
                }
                else
                {
                    foreach (DiscordUser user in await message.GetReactionsAsync(pollReaction.Emoji))
                    {
                        if (user == null)
                        {
                            return;
                        }

                        if (user.IsBot)
                        {
                            continue;
                        }

                        Vote userVote = this.PollService.GetVoteByUserOnPoll(poll, user.Id);

                        if (userVote == null)
                        {
                            await this.PollService.AddVote(poll, pollOptions.Where(x => x.EmojiName == pollReaction.Emoji.Name).FirstOrDefault(), user.Id);
                        }
                        else
                        {
                            if (userVote.PollId != poll.Id)
                            {
                                //await message.DeleteReactionsEmojiAsync(pollReaction.Emoji);
                            }

                            if (!pollOptions.Any(x => x.EmojiName == pollReaction.Emoji.Name))
                            {
                                //await message.DeleteReactionsEmojiAsync(pollReaction.Emoji);
                            }
                        }
                    }
                }
            }
        }
    }
}