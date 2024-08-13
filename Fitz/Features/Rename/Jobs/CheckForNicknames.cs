using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts;
using Fitz.Features.Rename.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fitz.Features.Rename.Jobs
{
    /// <summary>
    /// Check all discord guild members for nicknames. If they have a nickname, check to see if we have a record of it.
    /// If we do not have a record of it, set the nickname to the user's username.
    /// </summary>
    /// <param name="dClient"></param>
    /// <param name="renameService"></param>
    /// <param name="accountService"></param>
    public class CheckForNicknames(DiscordClient dClient, RenameService renameService, AccountService accountService, BotLog botLog) : ITimedJob
    {
        private readonly DiscordClient dClient = dClient;
        private readonly BotLog botLog = botLog;
        private readonly RenameService renameService = renameService;
        private readonly AccountService accountService = accountService;

        public ulong Emoji => ManageRoleEmojis.Warning;

        public string Interval => CronInterval.Every30Minutes;

        public async Task Execute()
        {
            try
            {
                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Checking for unset nicknames...");
                DiscordGuild waterbear = await dClient.GetGuildAsync(Variables.Guilds.Waterbear);
                IAsyncEnumerable<DiscordMember> members = waterbear.GetAllMembersAsync();
                List<DiscordMember> discordMembers = new List<DiscordMember>();
                await foreach (DiscordMember member in members)
                {
                    discordMembers.Add(member);
                    // If the member has a nickname, check to see if we have a record of it.
                    if (member.Nickname != null && member.Id != Users.Admin)
                    {
                        Renames rename = renameService.GetActiveRenameByAccountId(member.Id);
                        if (rename == null)
                        {
                            // If we don't have a record of it, set the nickname to the user's username.
                            await member.ModifyAsync(x => x.Nickname = member.Username);
                        }
                        else
                        {
                            if (member.Nickname != rename.NewName)
                            {
                                // If the nickname does not match the record, set the nickname to the record in the database.
                                await member.ModifyAsync(x => x.Nickname = rename.NewName);
                            }
                        }
                    }
                }
                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Finished checking for unset nicknames");
            }
            catch (DSharpPlus.Exceptions.UnauthorizedException)
            {
                Console.WriteLine("Cannot update admin username/nickname. This is not important.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}