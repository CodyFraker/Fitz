using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Rename.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Rename.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class RenameAdminCommands(RenameService renameService, AccountService accountService) : BaseCommandModule
    {
        private readonly RenameService renameService = renameService;
        private readonly AccountService accountService = accountService;

        [Command("renames")]
        [Description("Rename a user.")]
        public Task GetCurretRenames(CommandContext ctx)
        {
            this.renameService.GetExpiredRenames();
            return ctx.RespondAsync("Getting expired renames.");
        }

        [Command("listrenames")]
        [Description("List the renames for a particular user ID")]
        public async Task ListRenamesByUsers(CommandContext ctx, [RemainingText] ulong userId)
        {
            try
            {
                var renames = this.renameService.GetRenamesByAccountId(userId);
                if (renames.Count == 0)
                {
                    await ctx.RespondAsync("No renames found for this user.");
                }
                else
                {
                    string table = renames.Select(rename => new
                    {
                        User = this.accountService.FindAccount(rename.AffectedUserId).Username,
                        NewName = rename.NewName,
                        Days = rename.Days,
                        Expiration = rename.Expiration,
                    }).ToMarkdownTable();

                    DiscordEmbedBuilder renameEmbed = new DiscordEmbedBuilder
                    {
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                            Text = $"Renames",
                        },
                        Color = new DiscordColor(52, 114, 53),
                        Timestamp = DateTime.UtcNow,
                        Description = $"```md\n{table}\n```",
                    };
                    await ctx.RespondAsync(renameEmbed.Build());
                }
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync($"Error: {ex.Message}");
            }
        }

        [Command("resetrenames")]
        [Description("Iterate through all of the accounts and their renames. Reset them.")]
        public async Task ResetRenames(CommandContext ctx)
        {
            try
            {
                DiscordGuild guild = await ctx.Client.GetGuildAsync(Variables.Guilds.Waterbear);
                List<Renames> renames = this.renameService.GetAllRenames(null);
                string message = string.Empty;
                foreach (Renames rename in renames)
                {
                    Account affectedUser = this.accountService.FindAccount(rename.AffectedUserId);
                    DiscordMember discordMember = await guild.GetMemberAsync(affectedUser.Id);
                    if (discordMember != null)
                    {
                        var resetRename = discordMember.ModifyAsync(x => x.Nickname = discordMember.Username);
                        await resetRename;
                        if (resetRename.IsCompletedSuccessfully)
                        {
                            var setRenameStatusResult = await this.renameService.SetRenameStatus(rename.Id, RenameStatus.Expired);
                            if (setRenameStatusResult.Success)
                            {
                                message += $"Reset {affectedUser.Username}'s nickname to {discordMember.Username}\n";
                            }
                        }
                        else
                        {
                            message += $"Failed to reset {affectedUser.Username}'s nickname to {discordMember.Username}\n";
                        }
                    }
                }
                await ctx.RespondAsync(message);
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync($"Error: {ex.Message}");
            }
        }
    }
}