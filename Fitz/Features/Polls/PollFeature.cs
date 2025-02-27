using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Bank;
using Fitz.Features.Polls.Models;
using Fitz.Features.Polls.Polls;
using Fitz.Variables.Channels;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoteModel = Fitz.Features.Polls.Models.Vote;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Polls
{
    public class PollFeature(DiscordClient dClient, AccountService accountService, BankService bankService, PollService pollService, JobManager jobManager, IServiceScopeFactory scopeFactory) : Feature
    {
        private readonly CommandsNextExtension cNext = dClient.GetCommandsNext();
        private readonly JobManager jobManager = jobManager;
        private readonly SlashCommandsExtension slash = dClient.GetSlashCommands();
        private readonly AccountService accountService = accountService;
        private readonly PollService pollService = pollService;
        private readonly PollJob pollJob = new PollJob(dClient, pollService, scopeFactory);
        private readonly DiscordClient dClient = dClient;
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public override string Name => "Polls";

        public override string Description => "Create and manage polls.";

        public override Task Disable()
        {
            this.dClient.MessageReactionAdded -= this.OnReactionAddAsync;
            this.jobManager.RemoveJob(this.pollJob);
            this.cNext.UnregisterCommands<PollSlashCommands>();
            return base.Disable();
        }

        public override Task Enable()
        {
            this.dClient.MessageReactionAdded += this.OnReactionAddAsync;
            this.jobManager.AddJob(this.pollJob);
            // TODO: Fix register of slash commands and add modal context here too
            //this.slash.RegisterCommands<PollSlashCommands>();
            return base.Enable();
        }

        private async Task OnReactionAddAsync(DiscordClient dClient, MessageReactionAddEventArgs reaction)
        {
            // If Fitz reacted, ignore.
            if (reaction.User.IsBot)
            {
                return;
            }

            // Check to see if the reaction is in the pending polls channel
            if (reaction.Message.Channel.Id == Waterbear.PendingPolls || reaction.Message.Channel.Id == Waterbear.Polls)
            {
                // Get the poll from the database.
                Poll poll = this.pollService.GetPoll(reaction.Message.Id);
                if (poll != null)
                {
                    if (reaction.Message.Channel.Id == Waterbear.PendingPolls)
                    {
                        ManagePendingPollsVote(poll, reaction);
                    }
                    if (reaction.Message.Channel.Id == Waterbear.Polls)
                    {
                    }
                }
                return;
            }
        }

        private async void ManagePendingPollsVote(Poll poll, MessageReactionAddEventArgs reaction)
        {
            try
            {
                // Get the poll options
                List<PollOptions> pollOptions = this.pollService.GetPollOptions(poll);

                // Get the emoji from the reaction
                DiscordEmoji emoji = reaction.Emoji;
                DiscordUser user = reaction.User;
                DiscordMessage message = reaction.Message;

                // Check to see if we're adding a valid poll emoji.
                if (pollOptions.Any((x) => x.EmojiId == emoji.Id) || pollOptions.Any((x) => x.EmojiName == emoji.Name))
                {
                    PollOptions userOption = new();
                    if (emoji.Id == 0)
                    {
                        userOption = pollOptions.FirstOrDefault((x) => x.EmojiName == emoji.GetDiscordName());
                    }
                    else
                    {
                        userOption = pollOptions.FirstOrDefault((x) => x.EmojiId == emoji.Id);
                    }

                    // Check to see if reaction.user has an account
                    var account = this.accountService.FindAccount(user.Id);
                    if (account == null)
                    {
                        // User had no account to award beer. Ignore.
                        return;
                    }

                    // Use object to avoid namespace conflict
                    object userVote = this.pollService.GetVoteByUserOnPoll(poll, user.Id);
                    if (userVote == null)
                    {
                        // User has not provided a vote
                        // add beer to user account
                        await this.pollService.AddVote(poll, userOption, account);
                    }
                    else
                    {
                        // If user has voted but the choice has entered a null state, update their vote with whatever valid option they chose.
                        var vote = userVote as Models.Vote;
                        if (vote?.Choice == null)
                        {
                            // Update the vote
                            await this.pollService.UpdateVote(userVote, userOption.Id, account);
                            return;
                        }
                        else
                        {
                            // If the user has already voted, we need to remove their previous vote and update their vote with the new one.
                            // Remove their original reaction
                            PollOptions userOldOption = pollOptions.FirstOrDefault((x) => x.Id == vote.Choice);
                            if (userOldOption == null)
                            {
                                return;
                            }
                            if (userOldOption.EmojiId == 0)
                            {
                                try
                                {
                                    await message.DeleteReactionAsync(DiscordEmoji.FromUnicode(userOldOption.EmojiName), user);
                                }
                                catch (Exception ex)
                                {
                                    await message.DeleteReactionAsync(DiscordEmoji.FromName(dClient, userOldOption.EmojiName), user);
                                }
                            }
                            else
                            {
                                await message.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(dClient, userOldOption.EmojiId.Value), user);
                            }

                            // Update the vote
                            await this.pollService.UpdateVote(userVote, userOption.Id, account);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in ManagePendingPollsVote: {ex.Message}");
            }
        }

        private async void ManagePollChannelsVote(Poll poll, DiscordMessage message, DiscordUser user, DiscordEmoji emoji)
        {
            // Get the poll options
            List<PollOptions> pollOptions = this.pollService.GetPollOptions(poll);

            // Check to see if we're adding a valid poll emoji.
            if (pollOptions.Any((x) => x.EmojiId == emoji.Id) || pollOptions.Any((x) => x.EmojiName == emoji.Name))
            {
                PollOptions userOption = new();
                if (emoji.Id == 0)
                {
                    userOption = pollOptions.FirstOrDefault((x) => x.EmojiName == emoji.GetDiscordName());
                }
                else
                {
                    userOption = pollOptions.FirstOrDefault((x) => x.EmojiId == emoji.Id);
                }

                // Check to see if reaction.user has an account
                var account = this.accountService.FindAccount(user.Id);
                if (account == null)
                {
                    // User had no account to award beer. Ignore.
                    return;
                }
                object userVote = this.pollService.GetVoteByUserOnPoll(poll, user.Id);
                if (userVote == null)
                {
                    // User has not provided a vote
                    // add beer to user account
                    await this.pollService.AddVote(poll, userOption, account);
                }
                else
                {
                    // If user has voted but the choice has entered a null state, update their vote with whatever valid option they chose.
                    var vote = userVote as Models.Vote;
                    if (vote?.Choice == null)
                    {
                        // Update the vote
                        await this.pollService.UpdateVote(userVote, userOption.Id, account);
                        return;
                    }
                    else
                    {
                        // If the user has already voted, we need to remove their previous vote and update their vote with the new one.
                        // Remove their original reaction
                        PollOptions userOldOption = pollOptions.FirstOrDefault((x) => x.Id == vote.Choice);
                        if (userOldOption == null)
                        {
                            return;
                        }
                        if (userOldOption.EmojiId == 0)
                        {
                            try
                            {
                                await message.DeleteReactionAsync(DiscordEmoji.FromUnicode(userOldOption.EmojiName), user);
                            }
                            catch (Exception ex)
                            {
                                await message.DeleteReactionAsync(DiscordEmoji.FromName(dClient, userOldOption.EmojiName), user);
                            }
                        }
                        else
                        {
                            await message.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(dClient, userOldOption.EmojiId.Value), user);
                        }

                        // Update the vote
                        await this.pollService.UpdateVote(userVote, userOption.Id, account);
                    }
                }
            }
        }

        private DiscordEmbed NotifyPollCreatorEmbed(DiscordClient dClient, Poll poll, DiscordMessage? pollMessage)
        {
            // Set base embed color to white.
            DiscordColor embedColor = new DiscordColor(250, 250, 250);
            string embedTitle = string.Empty;
            string description = string.Empty;

            if (poll.Status == PollStatus.Approved)
            {
                embedColor = new DiscordColor(34, 206, 131);
                embedTitle = $"Poll #{poll.Id} was approved!";
                description += $"Question: {poll.Description}\n";
                description += $"When users react to your poll, you will obtain beer for their votes.\n";
            }
            if (poll.Status == PollStatus.Declined)
            {
                embedColor = new DiscordColor(255, 95, 31);
                embedTitle = $"Poll #{poll.Id} was denied...";
                description += $"Question: {poll.Description}\n";
            }

            description += $"\n";

            if (pollMessage != null)
            {
                description += $"\nView Poll -> {pollMessage.JumpLink}\n";
            }

            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, PollEmojis.InfoIcon).Url,
                    Text = $"Poll #{poll.Id} | {poll.Type}",
                },
                Color = embedColor,
                Timestamp = DateTime.UtcNow,
                Title = embedTitle,
                Description = description,
            };

            return pollEmbed;
        }
    }
}