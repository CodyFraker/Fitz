using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Rename.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public class RenameJob(DiscordClient dClient, RenameService renameService, AccountService accountService) : ITimedJob
    {
        private readonly DiscordClient dClient = dClient;
        private readonly RenameService renameService = renameService;
        private readonly AccountService accountService = accountService;

        public ulong Emoji => ManageRoleEmojis.Warning;

        public int Interval => 5;

        public async Task Execute()
        {
            //Get a list of all users who have been renamed
            List<Renames> renames = this.renameService.GetExpiredRenames();

            if (renames.Count == 0 || renames == null)
            {
                return;
            }

            foreach (Renames rename in renames.Where(x => x.Notified == false))
            {
                // if the rename has expired
                if (rename.Expiration < DateTime.Now)
                {
                    // Get the affected user
                    Account affectedUser = accountService.FindAccount(rename.AffectedUserId);
                    DiscordGuild waterbear = await this.dClient.GetGuildAsync(Variables.Guilds.Waterbear);
                    DiscordMember discordMember;
                    try
                    {
                        discordMember = await waterbear.GetMemberAsync(affectedUser.Id);
                    }
                    catch (NotFoundException e)
                    {
                        // Set the rename as notified.
                        await renameService.SetUserNotified(rename);
                        continue;
                    }
                    catch (Exception e)
                    {
                        // Set the rename as notified.
                        await renameService.SetUserNotified(rename);
                        continue;
                    }

                    // If the affected user exists
                    if (affectedUser != null)
                    {
                        // Check to see if the user is still in the server
                        if (discordMember != null)
                        {
                            // Check to see if their nickname is still the renamed name
                            if (discordMember.Nickname == rename.OldName)
                            {
                                continue;
                            }
                            else
                            {
                                // Check to see if we've notified the user or not.
                                if (!rename.Notified)
                                {
                                    // Reset the user's nickname with their original usename.
                                    await discordMember.ModifyAsync(x =>
                                    {
                                        x.Nickname = discordMember.Username;
                                    });

                                    // Set the rename as notified.
                                    await renameService.SetUserNotified(rename);

                                    // Send the user a message letting them know their name has been reset.
                                    await discordMember.SendMessageAsync(new DiscordMessageBuilder()
                                        .AddEmbed(renameEmbed(rename, affectedUser)));
                                }
                            }
                        }
                    }
                }
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