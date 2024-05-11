using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Features.Accounts;
using Fitz.Features.Bank;
using Fitz.Features.Polls.Models;
using Fitz.Features.Polls.Polls;
using Fitz.Variables.Channels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Polls
{
    public class PollFeature : Feature
    {
        private readonly CommandsNextExtension cNext;
        private readonly SlashCommandsExtension slash;
        private AccountService accountService;
        private PollService pollService;
        private BankService bankService;
        private readonly DiscordClient dClient;

        public PollFeature(DiscordClient dClient, AccountService accountService, BankService bankService, PollService pollService)
        {
            this.dClient = dClient;
            this.accountService = accountService;
            this.bankService = bankService;
            this.cNext = dClient.GetCommandsNext();
            this.slash = dClient.GetSlashCommands();
            this.pollService = pollService;
        }

        public override string Name => "Polls";

        public override string Description => "Create and manage polls.";

        public override Task Disable()
        {
            this.dClient.MessageReactionAdded -= this.OnReactionAddAsync;
            this.cNext.UnregisterCommands<PollSlashCommands>();
            return base.Disable();
        }

        public override Task Enable()
        {
            this.dClient.MessageReactionAdded += this.OnReactionAddAsync;
            // TODO: Fix register of slash commands and add modal context here too
            //this.slash.RegisterCommands<PollSlashCommands>();
            return base.Enable();
        }

        private async Task OnReactionAddAsync(DiscordClient dClient, MessageReactionAddEventArgs reaction)
        {
            // Check if the channel the reaction was added in is the polls channel
            if (reaction.Channel.Id != Waterbear.Polls)
            {
                return;
            }
            // We don't want to award bots beer. Or do we?
            if (reaction.User.IsBot)
            {
                return;
            }

            Poll poll = this.pollService.GetPoll(reaction.Message.Id);
            // Check to see if the message is a poll.
            if (poll == null)
            {
                return;
            }
            List<PollOptions> options = this.pollService.GetValidPollOptions(poll);

            // Check to see if we're adding a valid poll emoji.
            if (options.Any((x) => x.EmojiId == reaction.Emoji.Id) || options.Any((x) => x.Name == reaction.Emoji.Name))
            {
                PollOptions userOption = new PollOptions();
                if (reaction.Emoji.Id == 0)
                {
                    userOption = options.FirstOrDefault((x) => x.Name == reaction.Emoji.Name);
                }
                else
                {
                    userOption = options.FirstOrDefault((x) => x.EmojiId == reaction.Emoji.Id);
                }

                // Check to see if reaction.user has an account
                var account = this.accountService.FindAccount(reaction.User.Id);
                if (account == null)
                {
                    // User had no account to award beer. Ignore.
                    return;
                }
                Vote vote = await this.pollService.GetVoteByUserOnPoll(poll, reaction.User.Id);
                if (vote == null)
                {
                    // User has not provided a vote
                    // add beer to user account
                    // TODO: Create method in bankservice to award beer for voting instead of using AwardBonus.
                    await this.bankService.AwardBonus(reaction.User.Id, 1);
                    await this.pollService.AddVote(poll, userOption, account);
                }
                else
                {
                    // If user has voted but the choice has entered a null state, update their vote with whatever valid option they chose.
                    if (vote.Choice == null)
                    {
                        // Update the vote
                        await this.pollService.UpdateVote(vote, userOption.Id, account);
                        return;
                    }
                    else
                    {
                        // If the user has already voted, we need to remove their previous vote and update their vote with the new one.
                        // Remove their original reaction
                        PollOptions userOldOption = options.FirstOrDefault((x) => x.Id == vote.Choice.Value);
                        if (userOldOption == null)
                        {
                            return;
                        }
                        if (userOldOption.EmojiId == 0)
                        {
                            await reaction.Message.DeleteReactionAsync(DiscordEmoji.FromUnicode(userOldOption.Name), reaction.User);
                        }
                        else
                        {
                            await reaction.Message.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(dClient, userOldOption.EmojiId.Value), reaction.User);
                        }
                        var sdfsdfsdf = userOption;

                        await this.pollService.UpdateVote(vote, userOption.Id, account);
                    }
                }
            }
            else
            {
                await reaction.Message.DeleteReactionAsync(reaction.Emoji, reaction.User);
                return;
            }
        }
    }
}