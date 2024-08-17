using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Rename.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Fitz.Features.Rename.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class RenameSlashCommands(RenameService renameService, AccountService accountService, BankService bankService) : ApplicationCommandModule
    {
        private readonly RenameService renameService = renameService;
        private readonly AccountService accountService = accountService;
        private readonly BankService bankService = bankService;

        [SlashCommand("rename", "Rename a user within the guild.")]
        [RequireAccount]
        public async Task Rename(InteractionContext ctx,
            [Option("User", "Manage whose account?")] DiscordUser user = null,
            [Option("Name", "What should their new name be?")] string newName = null,
            [Option("Days", "What should their new name be?")] double days = 1)
        {
            // Check to see if a user was provided

            #region Error Checking

            if (user == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to specify a user.").AsEphemeral(true));
                return;
            }

            if (newName.Length > 32)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("That name is too long. The max length of a name is 32 characters.").AsEphemeral(true));
                return;
            }

            // Check to see if a new name was provided
            if (string.IsNullOrWhiteSpace(newName))
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to specify a new name for that user.").AsEphemeral(true));
                return;
            }

            // Check to see if the days is a valid number
            if (days <= 0)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to specify a valid number of days. 1-365").AsEphemeral(true));
                return;
            }

            #endregion Error Checking

            #region Check Accounts

            Account affectedUser = accountService.FindAccount(user.Id);
            Account requestingUser = accountService.FindAccount(ctx.User.Id);

            // Check to see if user account exists.
            if (affectedUser == null)
            {
                // TODO, allow users to bargin with Fitz to create an account for the user they want to rename
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("That user does not have an account. I cannot change their name right now.").AsEphemeral(true));
                return;
            }

            // Check to see if the requesting user has an account
            if (requestingUser == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to run `/signup` before you can interact with the rename feature.").AsEphemeral(true));
                return;
            }

            #endregion Check Accounts

            int renameCost = renameService.GenerateRenameCost(affectedUser, requestingUser, days, newName);

            // Check to see if requesting user has enough beer
            if (requestingUser.Beer < renameCost)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Changing their name would require you have {renameCost}. You instead only have {requestingUser.Beer}. idiot.").AsEphemeral(true));
                return;
            }

            Renames renameRequest = new Renames()
            {
                NewName = newName,
                AffectedUserId = affectedUser.Id,
                RequestedUserId = requestingUser.Id,
                Days = (int)days,
                Cost = renameCost,
            };

            int unique_id = 0;
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    rng.GetBytes(data);
                    unique_id = BitConverter.ToInt32(data, 0);
                    unique_id = Math.Abs(unique_id);
                }
            }

            await ctx.DeferAsync(true);

            // Check to see if the affected users already has an active rename
            List<Renames> renames = renameService.GetRenamesByAccountId(affectedUser.Id).OrderByDescending(x => x.Expiration).ToList();
            int buyoutCost = 0;
            DiscordButtonComponent accpetBtn = new(DiscordButtonStyle.Success, $"rename_confirm_{unique_id}", "Confirm", false);
            DiscordButtonComponent cancelBtn = new(DiscordButtonStyle.Danger, $"rename_cancel_{unique_id}", "Cancel", false);

            // If the user doesn't have any active renames
            if (renames.Count == 0 || renames == null)
            {
                renameRequest.StartDate = DateTime.Now;
                renameRequest.Expiration = DateTime.Now.AddDays(days);

                // Send a follow-up message to confirm the rename
                await ctx.FollowUpAsync(
                    new DiscordFollowupMessageBuilder()
                    .WithContent($"Renaming {user.Username} to {newName} for {days} day(s) will cost {renameCost} beer.\n" +
                    $"Start Date: {renameRequest.StartDate}\n" +
                    $"Expiration Date: {renameRequest.Expiration}\n" +
                    $"Do you want to proceed?")
                    .AddComponents(accpetBtn, cancelBtn)
                    .AsEphemeral(true));
            }
            else
            {
                foreach (Renames renameRequests in renames)
                {
                    buyoutCost += renameRequests.Cost;
                }
                buyoutCost += renameCost;

                string table = renames.Select(rename => new
                {
                    Name = rename.NewName,
                    Expires = rename.Expiration
                }).ToMarkdownTable();

                bool disableBuyOutBtn = requestingUser.Beer < buyoutCost;

                DiscordButtonComponent buyOutBtn = new(DiscordButtonStyle.Primary, $"buyout_confirm_{unique_id}", "Buy out", disableBuyOutBtn);
                DiscordButtonComponent acceptPendingBtn = new(DiscordButtonStyle.Success, $"rename_pending_confirm_{unique_id}", "Confirm", false);

                if (disableBuyOutBtn)
                {
                    await ctx.FollowUpAsync(
                        new DiscordFollowupMessageBuilder()
                        .WithContent($"{user.Username} already has an active rename.\n" +
                        $"Press 'Confirm' if you'd wish your rename request to start after {renames[0].Expiration}EST\n" +
                        $"You do not have enough beer to buyout the pending and active renames.\n" +
                        $"Buying out the renames will cost you **{buyoutCost}** beer.\n" +
                        $"Press 'Cancel' to cancel this request." +
                        $"\n\n```{table}```")
                        .AddComponents(acceptPendingBtn, buyOutBtn, cancelBtn)
                        .AsEphemeral(true));
                }
                else
                {
                    await ctx.FollowUpAsync(
                        new DiscordFollowupMessageBuilder()
                        .WithContent($"{user.Username} already has an active rename.\n" +
                        $"Press 'Confirm' if you'd wish your rename request to start after {renames[0].Expiration}EST\n" +
                        $"Press 'Buy Out' if you wish to override all current rename requests and start yours instead.\n" +
                        $"Buying out the renames will cost you **{buyoutCost}** beer.\n" +
                        $"Press 'Cancel' to cancel this request." +
                        $"\n\n```{table}```")
                        .AddComponents(acceptPendingBtn, buyOutBtn, cancelBtn)
                        .AsEphemeral(true));
                }
            }

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == $"rename_confirm_{unique_id}" && e.Interaction.User.Id == requestingUser.Id)
                {
                    try
                    {
                        var renameStatus = ctx.Guild.GetMemberAsync(affectedUser.Id).Result.ModifyAsync(x => x.Nickname = newName);
                        await renameStatus;
                        if (renameStatus.IsCompletedSuccessfully)
                        {
                            renameRequest.OldName = affectedUser.Username;
                            renameRequest.Status = RenameStatus.Active;
                            await renameService.RenameUserAsync(renameRequest);

                            await ctx.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent($"`{affectedUser.Username}` has been renamed to `{newName}` for the next {days} day(s). It costed you {renameCost} beer."));
                        }
                        else
                        {
                            await ctx.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent($"I couldn't complete that request for some reason. I didn't take any beer for the attempt. Good effort, though."));
                        }
                    }
                    catch (Exception ex)
                    {
                        await ctx.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent($"I couldn't complete that request for some reason. I didn't take any beer for the attempt. Good effort, though. {ex.Message}"));
                    }
                }
                else if (e.Id == $"rename_pending_confirm_{unique_id}" && e.Interaction.User.Id == requestingUser.Id)
                {
                    renameRequest.StartDate = renames[0].Expiration;
                    renameRequest.Expiration = renames[0].Expiration.Value.AddDays(days);
                    renameRequest.Status = RenameStatus.Pending;
                    renameRequest.Timestamp = DateTime.Now;
                    renameRequest.OldName = affectedUser.Username;
                    await renameService.RenameUserAsync(renameRequest);
                    await ctx.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent($"Your rename request has been set to start after {renames[0].Expiration}EST."));
                }
                else if (e.Id == $"buyout_confirm_{unique_id}" && e.Interaction.User.Id == requestingUser.Id)
                {
                    await renameService.BuyoutRenameRequests(affectedUser.Id);
                    renameRequest.StartDate = DateTime.Now;
                    renameRequest.Expiration = DateTime.Now.AddDays(days);
                    renameRequest.Timestamp = DateTime.Now;
                    renameRequest.Status = RenameStatus.Active;
                    renameRequest.OldName = affectedUser.Username;
                    await renameService.RenameUserAsync(renameRequest);
                    var renameStatus = ctx.Guild.GetMemberAsync(affectedUser.Id).Result.ModifyAsync(x => x.Nickname = newName);
                    await ctx.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent("Renames bought out."));
                }
                // if the cancel button was pressed
                else if (e.Id == $"rename_cancel_{unique_id}")
                {
                    await ctx.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent("Rename cancelled."));
                }
            };
        }
    }
}