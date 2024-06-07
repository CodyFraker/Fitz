using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Jobs
{
    //public sealed class CheckUserVotes(DiscordClient dClient, PollService pollService, BotLog botLog) : ITimedJob
    //{
    //    private readonly DiscordClient dClient = dClient;
    //    private readonly PollService PollService = pollService;
    //    private readonly BotLog botLog = botLog;

    //    public ulong Emoji => PollEmojis.InfoIcon;

    //    public int Interval => 60;

    //    public async Task Execute()
    //    {
    //        this.botLog.Information(LogConsoleSettings.Jobs, PollEmojis.InfoIcon, $"Checking user votes job");
    //        // Get poll channel
    //        DiscordChannel PollChannel = await dClient.GetChannelAsync(Variables.Channels.Waterbear.Polls);

    //        // Get all messages in the poll channel
    //        IAsyncEnumerable<DiscordMessage> pollChannelMessages = PollChannel.GetMessagesAsync();

    //        // Check to see if any messages are not in the database
    //        await foreach (DiscordMessage message in pollChannelMessages)
    //        {
    //            // Retrive poll from database by message ID
    //            Poll poll = this.PollService.GetPoll(message.Id);

    //            if (poll == null)
    //            {
    //                // If the poll is not in the database, delete the message
    //                await message.DeleteAsync("Deleting message from poll channel. Message was not a valid poll.");
    //            }
    //        }
    //    }
    //}
}