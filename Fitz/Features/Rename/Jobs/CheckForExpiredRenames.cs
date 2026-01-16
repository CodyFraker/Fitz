using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Rename.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Rename.Jobs
{
    public class CheckForExpiredRenames(DiscordClient dClient, RenameService renameService, AccountService accountService, BotLog botLog) : ITimedJob
    {
        private readonly DiscordClient dClient = dClient;
        private readonly BotLog botLog = botLog;
        private readonly RenameService renameService = renameService;
        private readonly AccountService accountService = accountService;

        public ulong Emoji => ManageRoleEmojis.Warning;

        public string Interval => CronInterval.Every5Minutes;

        public async Task Execute()
        {
            try
            {
                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Checking for expired renames...");
                List<Renames> renames = renameService.GetExpiredRenames();

                if (renames.Count == 0 || renames == null)
                {
                    this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Finished checking for expired renames.");
                    return;
                }

                DiscordGuild waterbear = await dClient.GetGuildAsync(Guilds.Waterbear);

                foreach (Renames rename in renames.Where(x => x.Notified == false && x.Status == RenameStatus.Active))
                {
                    Account affectedUser = accountService.FindAccount(rename.AffectedUserId);
                    DiscordMember discordMember;
                    try
                    {
                        discordMember = await waterbear.GetMemberAsync(affectedUser.Id);
                        
                        List<Renames> activeRenames = renameService.GetRenamesByAccountId(affectedUser.Id)
                            .Where(x => x.Status == RenameStatus.Active && x.Id != rename.Id)
                            .ToList();
                        
                        await renameService.SetUserNotified(rename);
                        await renameService.SetRenameStatus(rename.Id, RenameStatus.Expired);

                        if (activeRenames.Count == 0)
                        {
                            string nicknameToSet = string.IsNullOrWhiteSpace(rename.OldName) ? discordMember.Username : rename.OldName;
                            await discordMember.ModifyAsync(x => x.Nickname = nicknameToSet);
                            this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Reset nickname for user {affectedUser.Id} to {nicknameToSet}");

                            try
                            {
                                DiscordDmChannel userDMChannel = await discordMember.CreateDmChannelAsync();
                                await userDMChannel.SendMessageAsync(embed: renameEmbed(rename, affectedUser));
                            }
                            catch (Exception dmEx)
                            {
                                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Could not send DM to user {affectedUser.Id}: {dmEx.Message}");
                            }
                        }
                    }
                    catch (NotFoundException e)
                    {
                        await renameService.SetUserNotified(rename);
                        await renameService.SetRenameStatus(rename.Id, RenameStatus.Expired);
                        continue;
                    }
                    catch (Exception e)
                    {
                        this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"There was an error getting discord member. CheckForExpiredRenameJob: {e.Message}");
                        await renameService.SetUserNotified(rename);
                        await renameService.SetRenameStatus(rename.Id, RenameStatus.Expired);
                        continue;
                    }
                }

                List<Renames> pendingRenames = this.renameService.GetPendingRenames();
                if (pendingRenames != null && pendingRenames.Count > 0)
                {
                    var pendingRenamesToActivate = pendingRenames
                        .Where(x => x.StartDate.HasValue && x.StartDate.Value <= DateTime.Now)
                        .OrderBy(x => x.StartDate)
                        .ToList();

                    foreach (Renames pendingRename in pendingRenamesToActivate)
                    {
                        Account affectedUser = accountService.FindAccount(pendingRename.AffectedUserId);
                        Renames activeRename = renameService.GetActiveRenameByAccountId(affectedUser.Id);
                        
                        if (activeRename == null)
                        {
                            try
                            {
                                DiscordMember discordMember = await waterbear.GetMemberAsync(affectedUser.Id);
                                
                                await discordMember.ModifyAsync(x => x.Nickname = pendingRename.NewName);
                                await renameService.SetRenameStatus(pendingRename.Id, RenameStatus.Active);
                                
                                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Activated pending rename {pendingRename.Id} for user {affectedUser.Id} with nickname {pendingRename.NewName}");
                            }
                            catch (NotFoundException)
                            {
                                await renameService.SetRenameStatus(pendingRename.Id, RenameStatus.Expired);
                            }
                            catch (Exception ex)
                            {
                                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, $"Error activating pending rename {pendingRename.Id}: {ex.Message}");
                            }
                        }
                    }
                }

                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Finished checking for expired renames.");
            }
            catch (Exception ex)
            {
                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, ex.Message);
            }
        }

        private DiscordEmbed renameEmbed(Renames rename, Account affectedUser)
        {
            DiscordEmbedBuilder embed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, ManageRoleEmojis.Promotion).Url,
                    Text = $"Rename #{rename.Id}",
                },
                Color = new DiscordColor(108, 45, 199),
                Title = $"Rename Update",
                Description = $"Your nickname is no longer `{rename.NewName}`\n" +
                $"It has been reset back to `{rename.OldName}`.",
            };

            // TODO: Add a list of renames that have expired for the affected user.

            return embed.Build();
        }
    }
}