using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Fitz.Core.Api;
using Fitz.Core.Api.Models;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Rename.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Rename.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class RenameAdminCommands(FitzApiClient apiClient, AccountService accountService) : BaseCommandModule
    {
        private readonly FitzApiClient apiClient = apiClient;
        private readonly AccountService accountService = accountService;

        [Command("renames")]
        [Description("Rename a user.")]
        public async Task GetCurretRenames(CommandContext ctx)
        {
            var response = await apiClient.GetAsync<ApiResponse<List<RenameResponse>>>("/api/rename?status=Expired");
            await ctx.RespondAsync($"Found {response?.Data?.Count ?? 0} expired renames.");
        }

        [Command("listrenames")]
        [Description("List the renames for a particular user ID")]
        public async Task ListRenamesByUsers(CommandContext ctx, [RemainingText] ulong userId)
        {
            try
            {
                var response = await apiClient.GetAsync<ApiResponse<List<RenameResponse>>>($"/api/rename/user/{userId}");
                var renames = response?.Data ?? new List<RenameResponse>();
                if (renames.Count == 0)
                {
                    await ctx.RespondAsync("No renames found for this user.");
                }
                else
                {
                    string table = renames.Select(rename => new
                    {
                        User = this.accountService.FindAccount(rename.AffectedUserId)?.Username ?? "Unknown",
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
                var response = await apiClient.GetAsync<ApiResponse<List<RenameResponse>>>("/api/rename");
                var renames = response?.Data ?? new List<RenameResponse>();
                string message = string.Empty;
                foreach (var rename in renames)
                {
                    Account affectedUser = this.accountService.FindAccount(rename.AffectedUserId);
                    if (affectedUser == null) continue;

                    DiscordMember discordMember = await guild.GetMemberAsync(affectedUser.Id);
                    if (discordMember != null)
                    {
                        var resetRename = discordMember.ModifyAsync(x => x.Nickname = discordMember.Username);
                        await resetRename;
                        if (resetRename.IsCompletedSuccessfully)
                        {
                            var statusRequest = new UpdateRenameStatusRequest { Status = RenameStatus.Expired };
                            var statusResponse = await apiClient.PatchAsync<UpdateRenameStatusRequest, ApiResponse<RenameResponse>>($"/api/rename/{rename.Id}/status", statusRequest);
                            if (statusResponse != null && statusResponse.Success)
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