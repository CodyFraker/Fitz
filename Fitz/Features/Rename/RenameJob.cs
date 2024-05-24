using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Rename.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public class RenameJob : ITimedJob
    {
        private readonly DiscordClient dClient;
        private readonly RenameService renameService;
        private readonly AccountService accountService;

        public RenameJob(DiscordClient dClient, RenameService renameService, AccountService accountService)
        {
            this.dClient = dClient;
            this.renameService = renameService;
            this.accountService = accountService;
        }

        public ulong Emoji => ManageRoleEmojis.Warning;

        public int Interval => 25;

        public async Task Execute()
        {
            ////Get a list of all users who have been renamed
            //List<Renames> renames = this.renameService.GetExpiredRenames();

            //if (renames.Count == 0 || renames == null)
            //{
            //    return;
            //}

            //foreach (Renames rename in renames)
            //{
            //    // if the rename has expired
            //    if (rename.Expiration < DateTime.Now)
            //    {
            //        // Get the affected user
            //        Account affectedUser = accountService.FindAccount(rename.AffectedUserId);
            //        DiscordGuild waterbear = await this.dClient.GetGuildAsync(Variables.Guilds.Waterbear);
            //        DiscordMember discordMember = await waterbear.GetMemberAsync(affectedUser.Id);

            //        // If the affected user exists
            //        if (affectedUser != null)
            //        {
            //            // Reset the user's nickanem with their original usename.
            //            await discordMember.ModifyAsync(x =>
            //            {
            //                x.Nickname = discordMember.Username;
            //            });

            //            // Send the user a message letting them know their name has been reset.
            //            await discordMember.SendMessageAsync(new DiscordMessageBuilder()
            //                .AddEmbed(renameEmbed(rename, affectedUser)));
            //        }
            //    }
            //}
        }

        private DiscordEmbed renameEmbed(Renames rename, Account affectedUser)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
                    Text = $"Rename #{rename.Id}",
                },
                Color = new DiscordColor(108, 45, 199),
                Title = $"Rename Card",
                Description = $"Your nickname is no longer `{rename.NewName}`\n" +
                $"It has been reset back to `{rename.OldName}`.",
            };

            // TODO: Add a list of renames that have expired for the affected user.

            return embed.Build();
        }
    }
}