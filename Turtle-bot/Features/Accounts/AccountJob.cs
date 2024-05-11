using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    public class AccountJob : ITimedJob
    {
        private readonly AccountService accountService;
        private readonly DiscordClient dClient;
        private readonly BotLog botLog;

        public AccountJob(AccountService accountService, DiscordClient dClient, BotLog botLog)
        {
            this.accountService = accountService;
            this.dClient = dClient;
            this.botLog = botLog;
        }

        public ulong Emoji => ManageRoleEmojis.Warning;

        public int Interval => 1;

        public async Task Execute()
        {
            this.botLog.Information(LogConsoleSettings.Jobs, ManageRoleEmojis.Warning, $"Starting Account Pruning..");
            // Get all accounts
            List<Account> accounts = accountService.QueryAccounts();

            // Get the waterbear discord guild
            DiscordGuild guild = await dClient.GetGuildAsync(Guilds.Waterbear);

            // Get all members in the guild with the role "account"
            List<DiscordMember> accountMembers = guild.Members.Values.Where(x => x.Roles.Any(role => role.Id == Roles.Accounts)).ToList();

            // Get all members in the guild with the role "account" that are not in the database
            foreach (DiscordMember member in accountMembers)
            {
                if (accounts.Any(x => x.Id == member.Id))
                {
                    // If the user is already in the database, skip
                    continue;
                }
                else
                {
                    // If the user is not in the database, remove the role "account"
                    this.botLog.Information(LogConsoleSettings.Jobs, ManageRoleEmojis.Demotion, $"User {member.Username} does not have an account. Removing role.");
                    await member.RevokeRoleAsync(guild.GetRole(Roles.Accounts), "User had account role but did not have an account.");
                }

                // TODO: If a user an an account but doesn't have the role, add them to the role.
            }
        }
    }
}