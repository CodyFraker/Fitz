using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts.Update.Domain;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Update.Jobs
{
    public class AccountActivityJob : ITimedJob
    {
        private readonly UpdateAccountService _updateService;
        private readonly DiscordClient _client;
        private readonly BotLog _botLog;

        public ulong Emoji => ManageRoleEmojis.Warning;
        public int Interval => 60; // Run every 60 minutes

        public AccountActivityJob(UpdateAccountService updateService, DiscordClient client, BotLog botLog)
        {
            _updateService = updateService;
            _client = client;
            _botLog = botLog;
        }

        public async Task Execute()
        {
            try
            {
                _botLog.Information(LogConsoleSettings.Jobs, ManageRoleEmojis.Warning, 
                    "Updating user activity status...");
                
                // Get the Waterbear Discord guild
                DiscordGuild guild = await _client.GetGuildAsync(Guilds.Waterbear);
                if (guild == null)
                {
                    Log.Error("Failed to get Waterbear guild in AccountActivityJob");
                    return;
                }

                // Get all members in the guild
                List<DiscordMember> guildMembers = guild.Members.Values.ToList();
                
                // Update last seen date for online members
                foreach (DiscordMember member in guildMembers)
                {
                    if (member == null || member.IsBot)
                    {
                        continue;
                    }
                    
                    // Update last seen date for online users
                    if (member.Presence != null && member.Presence.Status != DiscordUserStatus.Offline)
                    {
                        await UpdateUserLastSeen(member.Id);
                    }
                }

                _botLog.Information(LogConsoleSettings.Jobs, ManageRoleEmojis.Warning, 
                    "Finished updating user activity status.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error executing AccountActivityJob");
                _botLog.Information(LogConsoleSettings.Jobs, ManageRoleEmojis.Warning, 
                    $"Error in account activity job: {ex.Message}");
            }
        }

        private async Task UpdateUserLastSeen(ulong userId)
        {
            try
            {
                UpdateAccountCommand command = new UpdateAccountCommand
                {
                    Id = userId,
                    LastSeenDate = DateTime.Now
                };

                await _updateService.UpdateAccountAsync(command);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to update last seen date for user {userId}");
            }
        }
    }
} 