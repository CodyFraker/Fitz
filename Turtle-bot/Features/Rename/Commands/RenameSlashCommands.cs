using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using System.Threading.Tasks;

namespace Fitz.Features.Rename.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class RenameSlashCommands : ApplicationCommandModule
    {
        private readonly RenameService renameService;
        private readonly AccountService accountService;
        private readonly BankService bankService;
        private const int RenameCost = 100;

        public RenameSlashCommands(RenameService renameService, AccountService accountService, BankService bankService)
        {
            this.renameService = renameService;
            this.accountService = accountService;
            this.bankService = bankService;
        }

        [SlashCommand("rename", "Rename a user within the guild.")]
        [RequireAccount]
        public async Task Rename(InteractionContext ctx,
            [Option("User", "Manage whose account?")] DiscordUser user = null,
            [Option("Name", "What should their new name be?")] string newName = null)
        {
            // Check to see if a user was provided
            if (user == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to specify a user.").AsEphemeral(true));
                return;
            }

            // Check to see if a new name was provided
            if (string.IsNullOrWhiteSpace(newName))
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to specify a new name for that user.").AsEphemeral(true));
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

            // Check to see if requesting user has enough beer
            if (requestingUser.Beer < RenameCost)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You don't have enough beer to rename that user.").AsEphemeral(true));
                return;
            }

            DiscordButtonComponent accpetBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "rename_confirm", "Confirm", false);
            DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "rename_cancel", "Cancel", false);

            await ctx.DeferAsync(true);

            // Send a follow-up message to confirm the rename
            await ctx.FollowUpAsync(
                new DiscordFollowupMessageBuilder()
                .WithContent($"Renaming {user.Username} to {newName} will cost {RenameCost} beer. Do you want to proceed?")
                .AddComponents(accpetBtn, cancelBtn)
                .AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == "rename_confirm")
                {
                    // Store rename in database. Deduct money.
                    await renameService.RenameUserAsync(affectedUser, requestingUser, newName);

                    string oldName = affectedUser.Username;

                    await ctx.Guild.GetMemberAsync(affectedUser.Id).Result.ModifyAsync(x => x.Nickname = newName);

                    await ctx.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent($"`{oldName}` has been renamed to `{newName}`."));
                }
                // if the cancel button was pressed
                else if (e.Id == "rename_cancel")
                {
                    await ctx.EditFollowupAsync(e.Message.Id, new DiscordWebhookBuilder().WithContent("Rename cancelled."));
                }
            };
        }
    }
}