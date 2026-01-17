using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Polls.Models;
using Fitz.Metrics;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Polls
{
    public class PollJob(DiscordClient dClient, PollService pollService, BotLog botLog, FitzMetrics? fitzMetrics = null) : ITimedJob
    {
        private readonly DiscordClient dClient = dClient;
        private readonly PollService PollService = pollService;
        private readonly BotLog botLog = botLog;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public ulong Emoji => PollEmojis.InfoIcon;

        public string Interval => CronInterval.Every5Minutes;

        public async Task Execute()
        {
            var stopwatch = Stopwatch.StartNew();
            var jobName = "PollJob";
            
            try
            {
                this.botLog.Information(LogConsoleSettings.Jobs, PollEmojis.InfoIcon, $"Starting Poll Job...");
                
                var polls = this.PollService.GetPolls();
                var activePolls = polls.Where(p => p.Status == PollStatus.Approved).Count();
                fitzMetrics?.SetPollsActive(activePolls);
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
                    // Check to see if all poll options were added to the message
                    if (message.Reactions == null || message.Reactions.Count == 0)
                    {
                        foreach (PollOptions option in pollOptions)
                        {
                            if (!message.Reactions.Any(x => x.Emoji.Name.Contains(option.EmojiName)))
                            {
                                if (option.EmojiName.Contains(':'))
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

                    // Iterate through each reaction on the poll.
                    foreach (DiscordReaction pollReaction in message.Reactions)
                    {
                        // Check to see the poll option is in the database for this particular poll. If not, remove it.
                        if (!pollOptions.Any(x => x.EmojiName.Contains(pollReaction.Emoji.Name)))
                        {
                            // Delete the reaction(s)
                            //await message.DeleteReactionsEmojiAsync(pollReaction.Emoji);
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
                    }
                }
            }
            this.botLog.Information(LogConsoleSettings.Jobs, PollEmojis.InfoIcon, $"Finished Poll Job");
            
            stopwatch.Stop();
            fitzMetrics?.RecordJobExecution(jobName, "success", stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                fitzMetrics?.RecordJobExecution(jobName, "error", stopwatch.Elapsed.TotalSeconds);
                fitzMetrics?.RecordJobExecutionError(jobName);
            }
        }
    }
}