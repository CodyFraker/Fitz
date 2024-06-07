using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using System.Collections.Generic;
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

        public int Interval => 60;

        public async Task Execute()
        {
            this.botLog.Information(LogConsoleSettings.Jobs, PollEmojis.InfoIcon, $"Starting Poll Job...");
            // Get poll channel
            DiscordChannel PollChannel = await dClient.GetChannelAsync(Variables.Channels.Waterbear.Polls);

            // Get all messages in the poll channel
            IAsyncEnumerable<DiscordMessage> pollChannelMessages = PollChannel.GetMessagesAsync();

            // Check to see if any messages are not in the database
            await foreach (DiscordMessage message in pollChannelMessages)
            {
                // Retrive poll from database by message ID
                Poll poll = this.PollService.GetPoll(message.Id);

                if (poll == null)
                {
                    // If the poll is not in the database, delete the message
                    await message.DeleteAsync("Deleting message from poll channel. Message was not a valid poll.");

                    // TODO: Determine if the message is supposed to be a poll that wasn't saved in the database.
                    return;
                }
                else
                {
                    // Retrieve all poll options from the database for this poll.
                    List<PollOptions> pollOptions = this.PollService.GetPollOptions(poll);

                    // Iterate through each reaction on the poll.
                    foreach (DiscordReaction pollReaction in message.Reactions)
                    {
                        // Check to see the poll option is in the database for this particular poll. If not, remove it.
                        if (!pollOptions.Any(x => x.EmojiName == pollReaction.Emoji.Name))
                        {
                            // Delete the reaction(s)
                            await message.DeleteReactionsEmojiAsync(pollReaction.Emoji);
                        }
                        else
                        {
                            // Get all users who've reacted to this poll.
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

                                // Check to see if the user has voted on the poll
                                Vote userVote = this.PollService.GetVoteByUserOnPoll(poll, user.Id);

                                // User has added their vote to the poll but Fitz didn't see when it happened.
                                if (userVote == null)
                                {
                                    // Add the vote to the database
                                    await this.PollService.AddVote(poll, pollOptions.Where(x => x.EmojiName == pollReaction.Emoji.Name).FirstOrDefault(), user.Id);
                                }
                                else
                                {
                                    if (userVote.PollId != poll.Id)
                                    {
                                        //// Delete the reaction
                                        //await message.DeleteReactionsEmojiAsync(pollReaction.Emoji);
                                    }

                                    // If the user's choice isn't in the pollOptions, we need to remove the reaction.
                                    if (!pollOptions.Any(x => x.EmojiName == pollReaction.Emoji.Name))
                                    {
                                        //// Delete the reaction
                                        //await message.DeleteReactionsEmojiAsync(pollReaction.Emoji);
                                    }
                                }
                            }
                        }

                        // Check to see if all poll options were added to the message
                        foreach (PollOptions option in pollOptions)
                        {
                            if (!message.Reactions.Any(x => x.Emoji.Name == option.EmojiName))
                            {
                                if (option.EmojiName.Contains(":"))
                                {
                                    await message.CreateReactionAsync(DiscordEmoji.FromName(dClient, option.EmojiName));
                                }
                                else
                                {
                                    await message.CreateReactionAsync(DiscordEmoji.FromName(dClient, $":{option.EmojiName}:"));
                                }
                            }
                        }
                    }
                }
            }
            this.botLog.Information(LogConsoleSettings.Jobs, PollEmojis.InfoIcon, $"Finished Poll Job");
        }
    }
}