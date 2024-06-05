using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    public class AccountJob(AccountService accountService, DiscordClient dClient, BotLog botLog) : ITimedJob
    {
        private readonly AccountService accountService = accountService;
        private readonly DiscordClient dClient = dClient;
        private readonly BotLog botLog = botLog;

        public ulong Emoji => ManageRoleEmojis.Warning;

        public int Interval => 25;

        public async Task Execute()
        {
            this.botLog.Information(LogConsoleSettings.Jobs, ManageRoleEmojis.Warning, $"Starting Account Pruning..");
            // Get all accounts
            List<Account> accounts = accountService.QueryAccounts();

            // Get the waterbear discord guild
            DiscordGuild guild = await dClient.GetGuildAsync(Guilds.Waterbear);

            // Get all members in the guild with the role "account"
            List<DiscordMember> accountMembers = guild.Members.Values.Where(x => x.Roles.Any(role => role.Id == Roles.Accounts)).ToList();

            List<DiscordMember> guildMembers = guild.Members.Values.ToList();

            // Get all members in the guild with the role "account" that are not in the database
            foreach (DiscordMember member in guildMembers)
            {
                if (member == null) continue;
                if (member.IsBot) continue;
                if (member.Roles.Any(role => role.Id == Roles.Accounts))
                {
                    // Check to see if the user is in the database
                    if (accounts.Any(x => x.Id == member.Id))
                    {
                        // If the user is already in the database and has the role.
                        continue;
                    }
                    else
                    {
                        // If the user is not in the database, remove the role "account"
                        this.botLog.Information(LogConsoleSettings.Jobs, ManageRoleEmojis.Demotion, $"User {member.Username} does not have an account. Removing role.");
                        await member.RevokeRoleAsync(guild.GetRole(Roles.Accounts), "User had account role but did not have an account.");
                    }
                }
                // if the user has an account but doesn't have the role, we need to add them to the role.
                if (accounts.Any(x => x.Id == member.Id) && !member.Roles.Any(role => role.Id == Roles.Accounts))
                {
                    this.botLog.Information(LogConsoleSettings.Jobs, ManageRoleEmojis.Promotion, $"{member.Username} had an account but didn't have the account role. Added role.");
                    await member.GrantRoleAsync(guild.GetRole(Roles.Accounts), $"{member.Username} had an account but didn't have the account role. Added role.");
                }
            }
        }
    }
}