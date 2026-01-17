using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Bank;
using Fitz.Metrics;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.HappyHour
{
    public class HappyHourJob(DiscordClient dClient, BankService bankService, FitzMetrics? fitzMetrics = null) : ITimedJob
    {
        private readonly DiscordClient dClient = dClient;
        private readonly BankService bankService = bankService;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public ulong Emoji => PollEmojis.HotTake;

        public string Interval => CronInterval.Every5Minutes;

        public async Task Execute()
        {
            var stopwatch = Stopwatch.StartNew();
            var jobName = "HappyHourJob";
            int beerAwarded = 0;
            int eventsTriggered = 0;
            
            try
            {
                // if time is between 8PM and 11PM EST
                // 19 = 7, 23 = 11
                if (DateTime.UtcNow.ToLocalTime().Hour >= 19 && DateTime.UtcNow.ToLocalTime().Hour <= 23)
                {
                    DiscordGuild waterbear = await this.dClient.GetGuildAsync(Guilds.Waterbear);
                    IReadOnlyList<DiscordChannel> guildChannels = await waterbear.GetChannelsAsync();
                    List<DiscordChannel> voiceChannels = guildChannels.Where(guildChannels => guildChannels.Type == DiscordChannelType.Voice).ToList();
                    foreach (DiscordChannel voiceChannel in voiceChannels)
                    {
                        if (voiceChannel.Users.Count == 0)
                        {
                            continue;
                        }
                        if (voiceChannel.Users.Count >= 2)
                        {
                            eventsTriggered++;
                            foreach (DiscordUser voiceChannelUser in voiceChannel.Users)
                            {
                                var happyHourResult = await this.bankService.AwardHappyHour(voiceChannelUser.Id);
                                if (happyHourResult.Success)
                                {
                                    beerAwarded++;
                                }
                            }
                        }
                    }
                    
                    if (eventsTriggered > 0)
                    {
                        fitzMetrics?.RecordHappyHourEvent(beerAwarded);
                    }
                }
                
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