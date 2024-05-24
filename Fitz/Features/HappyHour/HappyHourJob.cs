﻿using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Bank;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.HappyHour
{
    public class HappyHourJob : ITimedJob
    {
        private readonly DiscordClient dClient;
        private readonly BankService bankService;

        public HappyHourJob(DiscordClient dClient, BankService bankService)
        {
            this.dClient = dClient;
            this.bankService = bankService;
        }

        public ulong Emoji => PollEmojis.HotTake;

        public int Interval => 5;

        public async Task Execute()
        {
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
                            return;
                        }
                        if (voiceChannel.Users.Count >= 2)
                        {
                            foreach (DiscordUser voiceChannelUser in voiceChannel.Users)
                            {
                                var happyHourResult = await this.bankService.AwardHappyHour(voiceChannelUser.Id, 6);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Not happy hour");
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}