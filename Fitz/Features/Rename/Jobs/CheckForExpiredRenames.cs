using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Fitz.Core.Discord;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Rename.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

        public int Interval => 5;

        public async Task Execute()
        {
            try
            {
                this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Checking for expired renames...");
                //Get a list of all users who have been renamed
                List<Renames> renames = renameService.GetExpiredRenames();

                if (renames.Count == 0 || renames == null)
                {
                    this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "Finished checking for expired renames.");
                    return;
                }

                // Iterate through each rename where the user has not been notified and the rename is still active.
                foreach (Renames rename in renames.Where(x => x.Notified == false && x.Status == RenameStatus.Active))
                {
                    // Get the affected user
                    Account affectedUser = accountService.FindAccount(rename.AffectedUserId);
                    DiscordGuild waterbear = await dClient.GetGuildAsync(Variables.Guilds.Waterbear);
                    DiscordMember discordMember;
                    try
                    {
                        discordMember = await waterbear.GetMemberAsync(affectedUser.Id);
                        await renameService.SetUserNotified(rename);
                        await renameService.SetRenameStatus(rename.Id, RenameStatus.Expired);
                    }
                    catch (NotFoundException e)
                    {
                        // Set the rename as notified. The user is no longer in the guild.
                        await renameService.SetUserNotified(rename);
                        await renameService.SetRenameStatus(rename.Id, RenameStatus.Expired);
                        continue;
                    }
                    catch (Exception e)
                    {
                        this.botLog.Information(LogConsoleSettings.RenameLog, ManageRoleEmojis.Warning, "There was an error getting discord member. CheckForExpiredRenameJob");
                        // Set the rename as notified. There was an error getting the member.
                        await renameService.SetUserNotified(rename);
                        await renameService.SetRenameStatus(rename.Id, RenameStatus.Expired);
                        continue;
                    }
                }

                List<Renames> pendingRenames = this.renameService.GetPendingRenames();
                if (pendingRenames.Count == 0 || pendingRenames == null)
                {
                    return;
                }
                else
                {
                    Renames nextRename = pendingRenames.OrderBy(x => x.StartDate).First();
                    if (nextRename != null)
                    {
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