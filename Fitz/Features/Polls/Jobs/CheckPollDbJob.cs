using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using Hangfire;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Jobs
{
    public class CheckPollDbJob(DiscordClient dClient, PollService pollService, BotLog botLog)
    {
        private readonly DiscordClient dClient = dClient;
        private readonly PollService PollService = pollService;
        private readonly BotLog botLog = botLog;

        public async Task Execute(DiscordMessage message)
        {
            this.botLog.Information(LogConsoleSettings.Jobs, PollEmojis.InfoIcon, $"Checking if polls weren't stored in database...");
            // Retrive poll from database by message ID
            Poll poll = this.PollService.GetPoll(message.Id);

            // If the poll is not in the database, delete the message
            if (poll == null)
            {
                await message.DeleteAsync("Deleting message from poll channel. Message was not a valid poll.");
                return;
            }
            else
            {
                BackgroundJob.Enqueue<CheckPollOptionsDb>(x => x.Execute(message, poll));
            }
        }
    }
}