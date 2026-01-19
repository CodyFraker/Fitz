using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using Fitz.Metrics;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class CheckForNicknames(DiscordClient dClient, RenameService renameService, AccountService accountService, BotLog botLog, FitzMetrics? fitzMetrics = null) : ITimedJob
    {
        private readonly DiscordClient dClient = dClient;
        private readonly BotLog botLog = botLog;
        private readonly RenameService renameService = renameService;
        private readonly AccountService accountService = accountService;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public ulong Emoji => ManageRoleEmojis.Warning;

        public string Interval => CronInterval.Every25Minutes;

        public async Task Execute()
        {
            var stopwatch = Stopwatch.StartNew();
            var jobName = "CheckForNicknames";
            int successCount = 0;
            int errorCount = 0;
            
            try
            {
                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Checking for unset nicknames...");
                
                DiscordGuild waterbear;
                try
                {
                    waterbear = await dClient.GetGuildAsync(Variables.Guilds.Waterbear);
                    if (waterbear == null)
                    {
                        this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Guild not accessible, skipping nickname check.");
                        stopwatch.Stop();
                        fitzMetrics?.RecordJobExecution(jobName, "skipped", stopwatch.Elapsed.TotalSeconds);
                        return;
                    }
                }
                catch (NotFoundException)
                {
                    this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Guild not accessible, skipping nickname check.");
                    stopwatch.Stop();
                    fitzMetrics?.RecordJobExecution(jobName, "skipped", stopwatch.Elapsed.TotalSeconds);
                    return;
                }
                catch (Exception ex)
                {
                    this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Error accessing guild: {ex.Message}");
                    stopwatch.Stop();
                    fitzMetrics?.RecordJobExecution(jobName, "error", stopwatch.Elapsed.TotalSeconds);
                    fitzMetrics?.RecordJobExecutionError(jobName);
                    return;
                }
                
                IAsyncEnumerable<DiscordMember> members = waterbear.GetAllMembersAsync();
                await foreach (DiscordMember member in members)
                {
                    try
                    {
                        RenamesEntity rename = renameService.GetActiveRenameByAccountId(member.Id);
                        
                        if (rename != null)
                        {
                            if (string.IsNullOrWhiteSpace(rename.NewName))
                            {
                                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Invalid rename.NewName for member {member.Id}, skipping.");
                                continue;
                            }
                            
                            bool hasNickname = !string.IsNullOrEmpty(member.Nickname);
                            if (!hasNickname || member.Nickname != rename.NewName)
                            {
                                await member.ModifyAsync(x => x.Nickname = rename.NewName);
                                successCount++;
                                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Set nickname for member {member.Id} to {rename.NewName}");
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(member.Nickname))
                            {
                                if (string.IsNullOrEmpty(member.Username))
                                {
                                    this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Invalid username for member {member.Id}, skipping.");
                                    continue;
                                }
                                
                                if (member.Nickname != member.Username)
                                {
                                    //await member.ModifyAsync(x => x.Nickname = member.Username);
                                    successCount++;
                                    this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Reset nickname for member {member.Id} to {member.Username}");
                                }
                            }
                        }
                    }
                    catch (NotFoundException)
                    {
                        errorCount++;
                        this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Member {member.Id} no longer exists in guild, skipping.");
                    }
                    catch (UnauthorizedException ex)
                    {
                        errorCount++;
                        this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Permission error modifying nickname for member {member.Id}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Error processing member {member.Id}: {ex.Message}");
                    }
                }
                
                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Finished checking for unset nicknames. Success: {successCount}, Errors: {errorCount}");
                
                stopwatch.Stop();
                fitzMetrics?.RecordJobExecution(jobName, "success", stopwatch.Elapsed.TotalSeconds);
                fitzMetrics?.RecordRenameJobExecution("check_nicknames");
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                fitzMetrics?.RecordJobExecution(jobName, "error", stopwatch.Elapsed.TotalSeconds);
                fitzMetrics?.RecordJobExecutionError(jobName);
                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Fatal error in CheckForNicknames: {e.Message}");
            }
        }
    }
}