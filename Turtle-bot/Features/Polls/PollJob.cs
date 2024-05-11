using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Polls.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Polls
{
    public class PollJob : ITimedJob
    {
        private readonly DiscordClient dClient;
        private readonly PollService PollService;

        public PollJob(DiscordClient dClient, PollService pollService)
        {
            this.dClient = dClient;
            PollService = pollService;
        }

        public ulong Emoji => PollEmojis.InfoIcon;

        public int Interval => 5;

        public async Task Execute()
        {
            // Get all polls
            List<Poll> polls = PollService.GetPolls();

            // Get poll channel
            DiscordChannel PollChannel = await dClient.GetChannelAsync(Variables.Channels.Waterbear.Polls);

            // Get all messages in the poll channel
            IAsyncEnumerable<DiscordMessage> messages = PollChannel.GetMessagesAsync();

            // Check to see if any messages are not in the database
            await foreach (DiscordMessage message in messages)
            {
                if (!polls.Any(p => p.MessageId == message.Id))
                {
                    // Delete the message if its not in the polls table.
                    await message.DeleteAsync();
                }

                // TODO: Check to see if any votes have been made to the poll that have not been recorded.
            }
        }
    }
}