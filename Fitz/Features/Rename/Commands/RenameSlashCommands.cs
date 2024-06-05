using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using System;
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

            int renameCost = renameService.GenerateRenameCost(affectedUser, requestingUser, days, newName);

            // Check to see if requesting user has enough beer
            if (requestingUser.Beer < renameCost)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Changing their name would require you have {renameCost}. You instead only have {requestingUser.Beer}. idiot.").AsEphemeral(true));
                return;
            }

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

            DiscordButtonComponent accpetBtn = new(DiscordButtonStyle.Success, $"rename_confirm_{unique_id}", "Confirm", false);
            DiscordButtonComponent cancelBtn = new(DiscordButtonStyle.Danger, $"rename_cancel_{unique_id}", "Cancel", false);

            await ctx.DeferAsync(true);

            // Send a follow-up message to confirm the rename
            await ctx.FollowUpAsync(
                new DiscordFollowupMessageBuilder()
                .WithContent($"Renaming {user.Username} to {newName} for {days} day(s) will cost {renameCost} beer. Do you want to proceed?")
                .AddComponents(accpetBtn, cancelBtn)
                .AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == $"rename_confirm_{unique_id}" && e.Interaction.User.Id == requestingUser.Id)
                {
                    var renameStatus = ctx.Guild.GetMemberAsync(affectedUser.Id).Result.ModifyAsync(x => x.Nickname = newName);
                    await renameStatus;
                    if (renameStatus.IsCompletedSuccessfully)
                    {
                        // Store rename in database. Deduct money.
                        await renameService.RenameUserAsync(affectedUser, requestingUser, newName, (int)days, renameCost);

                        string oldName = affectedUser.Username;

                        await ctx.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent($"`{oldName}` has been renamed to `{newName}` for the next {days} day(s). It costed you {renameCost} beer."));
                    }
                    else
                    {
                        await ctx.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent($"I couldn't complete that request for some reason. I didn't take any beer for the attempt. Good effort, though."));
                    }
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