using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Fitz.Core.Api;
using Fitz.Core.Api.Models;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Features.Accounts;
using Fitz.Database.Entities;
using Fitz.Features.Polls.Polls;
using Fitz.Variables;
using Fitz.Variables.Channels;
using Fitz.Variables.Emojis;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Polls
{
    public class PollFeature(DiscordClient dClient, BotLog botLog, AccountService accountService, PollService pollService, FitzApiClient apiClient) : Feature
    {
        private readonly CommandsNextExtension cNext = dClient.GetCommandsNext();
        private readonly SlashCommandsExtension slash = dClient.GetSlashCommands();
        private readonly AccountService accountService = accountService;
        private readonly PollService pollService = pollService;
        private readonly PollJob pollJob = new PollJob(dClient, pollService, botLog);
        private readonly FitzApiClient apiClient = apiClient;
        private readonly DiscordClient dClient = dClient;

        public override string Name => "Polls";

        public override string Description => "Create and manage polls.";

        public override Task Disable()
        {
            this.dClient.MessageReactionAdded -= this.OnReactionAddAsync;
            RecurringJob.RemoveIfExists("PollJob");
            this.cNext.UnregisterCommands<PollSlashCommands>();
            return base.Disable();
        }

        public override Task Enable()
        {
            this.dClient.MessageReactionAdded += this.OnReactionAddAsync;
            RecurringJob.AddOrUpdate("PollJob", () => this.pollJob.Execute(), this.pollJob.Interval);
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
                var pollResponse = await apiClient.GetAsync<ApiResponse<PollResponse>>($"/api/polls/message/{reaction.Message.Id}");
                if (pollResponse == null || !pollResponse.Success || pollResponse.Data == null)
                {
                    return;
                }

                var pollData = pollResponse.Data;
                var poll = new Poll
                {
                    Id = pollData.Id,
                    AccountId = pollData.AccountId,
                    MessageId = pollData.MessageId,
                    Question = pollData.Question,
                    Type = pollData.Type,
                    Status = pollData.Status,
                    EvaluatedOn = pollData.EvaluatedOn,
                    SubmittedOn = pollData.SubmittedOn
                };

                var optionsResponse = await apiClient.GetAsync<ApiResponse<List<PollOptionResponse>>>($"/api/polls/{poll.Id}/options");
                var pollOptionsData = optionsResponse?.Data ?? new List<PollOptionResponse>();
                var pollOptions = pollOptionsData.Select(o => new PollOptions
                {
                    Id = o.Id,
                    PollId = o.PollId,
                    Answer = o.Answer,
                    EmojiName = o.EmojiName,
                    EmojiId = o.EmojiId
                }).ToList();

                #region Pending Polls

                if (poll.Status == PollStatus.Pending)
                {
                    List<DiscordMember> pollApprovers = reaction.Message.Channel.Users.Where(DiscordMember => !DiscordMember.IsBot).ToList();
                    IReadOnlyList<DiscordUser> approvalReactions = await reaction.Message.GetReactionsAsync(DiscordEmoji.FromGuildEmote(dClient, PollEmojis.Yes));
                    IReadOnlyList<DiscordUser> denyReactions = await reaction.Message.GetReactionsAsync(DiscordEmoji.FromGuildEmote(dClient, PollEmojis.No));
                    DiscordChannel pollChannel = await this.dClient.GetChannelAsync(Waterbear.Polls);

                    // Approved
                    if (approvalReactions.Where(x => !x.IsBot).Count() >= 2)
                    {
                        var evaluateRequest = new EvaluatePollRequest { Status = PollStatus.Approved };
                        var approvalResponse = await apiClient.PatchAsync<EvaluatePollRequest, ApiResponse<PollResponse>>($"/api/polls/{poll.Id}/evaluate", evaluateRequest);
                        if (approvalResponse != null && approvalResponse.Success && approvalResponse.Data != null)
                        {
                            poll.Status = approvalResponse.Data.Status;
                            poll.EvaluatedOn = approvalResponse.Data.EvaluatedOn;
                            if (pollChannel != null)
                            {
                                // Send the poll to the poll channel
                                DiscordMessage pollMessage = await pollChannel.SendMessageAsync(this.pollService.GeneratePollEmbed(dClient, poll, pollOptions));
                                foreach (PollOptions option in pollOptions)
                                {
                                    if (option.EmojiId == 0)
                                    {
                                        await pollMessage.CreateReactionAsync(DiscordEmoji.FromName(dClient, option.EmojiName));
                                        await Task.Delay(250);
                                    }
                                    else if (option.EmojiId != 0)
                                    {
                                        await pollMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(dClient, option.EmojiId.Value));
                                        await Task.Delay(250);
                                    }
                                }
                                // Update the poll's message.id to the new message ID.
                                poll.MessageId = pollMessage.Id;

                                // Update the pending poll message to show it was approved.
                                await reaction.Message.ModifyAsync(this.pollService.UpdatePollEmbed(dClient, poll, pollOptions, pollMessage));
                                await reaction.Message.DeleteAllReactionsAsync();

                                // Send a notification to the user who submitted the poll.
                                DiscordMember pollCreator = await this.dClient.Guilds.Where(x => x.Value.Id == Variables.Guilds.Waterbear).FirstOrDefault().Value.GetMemberAsync(poll.AccountId);
                                if (pollCreator != null)
                                {
                                    DiscordDmChannel pollCreatorDm = await pollCreator.CreateDmChannelAsync();
                                    await pollCreatorDm.SendMessageAsync(this.NotifyPollCreatorEmbed(dClient, poll, pollMessage));
                                }
                            }
                        }
                    }
                    // Denied
                    else if (denyReactions.Where(x => !x.IsBot).Count() >= 2)
                    {
                        var evaluateRequest = new EvaluatePollRequest { Status = PollStatus.Declined };
                        var denyResponse = await apiClient.PatchAsync<EvaluatePollRequest, ApiResponse<PollResponse>>($"/api/polls/{poll.Id}/evaluate", evaluateRequest);
                        if (denyResponse != null && denyResponse.Success && denyResponse.Data != null)
                        {
                            poll.Status = denyResponse.Data.Status;
                            poll.EvaluatedOn = denyResponse.Data.EvaluatedOn;
                            if (pollChannel != null)
                            {
                                // Update the pending poll message to show it was denied.
                                await reaction.Message.ModifyAsync(this.pollService.UpdatePollEmbed(dClient, poll, pollOptions, null));
                                await reaction.Message.DeleteAllReactionsAsync();

                                // Send a notification to the user who submitted the poll.
                                DiscordMember pollCreator = await this.dClient.Guilds.Where(x => x.Value.Id == Variables.Guilds.Waterbear).FirstOrDefault().Value.GetMemberAsync(poll.AccountId);
                                if (pollCreator != null)
                                {
                                    DiscordDmChannel pollCreatorDm = await pollCreator.CreateDmChannelAsync();
                                    await pollCreatorDm.SendMessageAsync(this.NotifyPollCreatorEmbed(dClient, poll, null));
                                }
                            }
                        }
                    }
                }

                #endregion Pending Polls

                #region Poll Vote

                if (reaction.Message.ChannelId == Waterbear.Polls && poll.Status == PollStatus.Approved)
                {
                    // Check to see if we're adding a valid poll emoji.
                    if (pollOptions.Any((x) => x.EmojiId == reaction.Emoji.Id) || pollOptions.Any((x) => x.EmojiName == reaction.Emoji.Name))
                    {
                        PollOptions userOption = new();
                        if (reaction.Emoji.Id == 0)
                        {
                            userOption = pollOptions.FirstOrDefault((x) => x.EmojiName == reaction.Emoji.GetDiscordName());
                        }
                        else
                        {
                            userOption = pollOptions.FirstOrDefault((x) => x.EmojiId == reaction.Emoji.Id);
                        }

                        // Check to see if reaction.user has an account
                        var account = this.accountService.FindAccount(reaction.User.Id);
                        if (account == null)
                        {
                            // User had no account to award beer. Ignore.
                            return;
                        }

                        var votesResponse = await apiClient.GetAsync<ApiResponse<List<VoteResponse>>>($"/api/polls/{poll.Id}/votes");
                        var votes = votesResponse?.Data ?? new List<VoteResponse>();
                        var vote = votes.FirstOrDefault(v => v.UserId == reaction.User.Id);

                        if (vote == null)
                        {
                            // User has not provided a vote
                            var addVoteRequest = new AddVoteRequest
                            {
                                UserId = reaction.User.Id,
                                OptionId = userOption.Id
                            };
                            await apiClient.PostAsync<AddVoteRequest, ApiResponse<object>>($"/api/polls/{poll.Id}/vote", addVoteRequest);
                        }
                        else
                        {
                            // If user has voted but the choice has entered a null state, update their vote with whatever valid option they chose.
                            if (vote.Choice == null)
                            {
                                // Update the vote
                                var updateVoteRequest = new UpdateVoteRequest
                                {
                                    UserId = reaction.User.Id,
                                    OptionId = userOption.Id
                                };
                                await apiClient.PutAsync<UpdateVoteRequest, ApiResponse<VoteResponse>>($"/api/polls/{poll.Id}/vote", updateVoteRequest);
                                return;
                            }
                            else
                            {
                                // If the user has already voted, we need to remove their previous vote and update their vote with the new one.
                                // Remove their original reaction
                                PollOptions userOldOption = pollOptions.FirstOrDefault((x) => x.Id == vote.Choice.Value);
                                if (userOldOption == null)
                                {
                                    return;
                                }
                                if (userOldOption.EmojiId == 0)
                                {
                                    try
                                    {
                                        await reaction.Message.DeleteReactionAsync(DiscordEmoji.FromUnicode(userOldOption.EmojiName), reaction.User);
                                    }
                                    catch (Exception ex)
                                    {
                                        await reaction.Message.DeleteReactionAsync(DiscordEmoji.FromName(dClient, userOldOption.EmojiName), reaction.User);
                                    }
                                }
                                else
                                {
                                    await reaction.Message.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(dClient, userOldOption.EmojiId.Value), reaction.User);
                                }

                                var updateVoteRequest = new UpdateVoteRequest
                                {
                                    UserId = reaction.User.Id,
                                    OptionId = userOption.Id
                                };
                                await apiClient.PutAsync<UpdateVoteRequest, ApiResponse<VoteResponse>>($"/api/polls/{poll.Id}/vote", updateVoteRequest);
                            }
                        }
                    }
                    else
                    {
                        await reaction.Message.DeleteReactionAsync(reaction.Emoji, reaction.User);
                        return;
                    }
                }

                #endregion Poll Vote
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
                description += $"Question: {poll.Question}\n";
                description += $"When users react to your poll, you will obtain beer for their votes.\n";
            }
            if (poll.Status == PollStatus.Declined)
            {
                embedColor = new DiscordColor(255, 95, 31);
                embedTitle = $"Poll #{poll.Id} was denied...";
                description += $"Question: {poll.Question}\n";
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