using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;
using DSharpPlus.SlashCommands;
using Fitz.Core.Models;
using Fitz.Core.Services.Settings;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Polls
{
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    public class PollModalCommands(PollService pollService, SettingsService settingsService) : ModalCommandModule
    {
        private readonly PollService pollService = pollService;
        private readonly Settings settings = settingsService.GetSettings();

        #region Number

        [ModalCommand("gen_number")]
        public async Task GenerateNumberPoll(ModalContext ctx, string question, string choices)
        {
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
            string[] answerOptions = choices.Split(',');
            answerOptions = answerOptions.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            List<PollOptions> pollOptions = new List<PollOptions>();

            if (answerOptions.Length > 10 || answerOptions.Length <= 1)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"You need at least 2 options but no more than 10. You provided {answerOptions.Length} option(s).")
                    .AsEphemeral(true));
                return;
            }

            for (int i = 0; i < answerOptions.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":one:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":one:").Id,
                        });
                        break;

                    case 1:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":two:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":two:").Id,
                        });
                        break;

                    case 2:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":three:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":three:").Id,
                        });
                        break;

                    case 3:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":four:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":four:").Id,
                        });
                        break;

                    case 4:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":five:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":five:").Id,
                        });
                        break;

                    case 5:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":six:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":six:").Id,
                        });
                        break;

                    case 6:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":seven:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":seven:").Id,
                        });
                        break;

                    case 7:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":eight:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":eight:").Id,
                        });
                        break;

                    case 8:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":nine:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":nine:").Id,
                        });
                        break;

                    case 9:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":keycap_ten:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":keycap_ten:").Id,
                        });
                        break;
                }
            }

            DiscordButtonComponent accpetBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"number_poll_confirm_{unique_id}", "Confirm", false);
            DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"number_poll_cancel_{unique_id}", "Cancel", false);

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Here is what the poll will look like. If everything looks good, hit 'Confirm' and you will be charged {settings.PollSubmittedPenalty} beer for the poll submission. Clicking cancel will *NOT* submit the poll and you will be forced to start over.")
                .AddEmbed(GeneratePollEmbed(ctx, question, pollOptions, PollType.Number))
                .AddComponents(cancelBtn, accpetBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == $"number_poll_confirm_{unique_id}")
                {
                    // Notify the user that the poll is being submitted.
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Submitting number poll..."));
                    var sendPendingPoll = await SendPendingPoll(ctx, question, pollOptions, PollType.Number);
                    if (sendPendingPoll.Success)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I will evaluate if the poll is worthy of posting. If so, you will gain beer."));
                        return;
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Poll Submission Failed. {sendPendingPoll.Message}"));
                        return;
                    }
                }
                // if the cancel button was pressed
                else if (e.Id == $"number_poll_cancel_{unique_id}")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll not created."));
                }
            };
        }

        #endregion Number

        #region Color

        [ModalCommand("generate_color_poll")]
        public async Task GenerateColorPoll(ModalContext ctx, string question, string choices)
        {
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
            string[] answerOptions = choices.Split(',');
            answerOptions = answerOptions.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            List<PollOptions> pollOptions = new List<PollOptions>();

            if (answerOptions.Length > 9)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"You can only have a maximum of 9 options for color polls. You provided {answerOptions.Length}")
                    .AsEphemeral(true));
                return;
            }

            for (int i = 0; i < answerOptions.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":blue_circle:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":blue_circle:").Id,
                        });
                        break;

                    case 1:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":green_circle:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":green_circle:").Id,
                        });
                        break;

                    case 2:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":orange_circle:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":orange_circle:").Id,
                        });
                        break;

                    case 3:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":purple_circle:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":purple_circle:").Id,
                        });
                        break;

                    case 4:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":red_circle:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":red_circle:").Id,
                        });
                        break;

                    case 5:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":yellow_circle:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":yellow_circle:").Id,
                        });
                        break;

                    case 6:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":brown_circle:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":brown_circle:").Id,
                        });
                        break;

                    case 7:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":black_circle:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":black_circle:").Id,
                        });
                        break;

                    case 8:
                        pollOptions.Add(new PollOptions
                        {
                            Answer = answerOptions[i],
                            EmojiName = ":white_circle:",
                            EmojiId = DiscordEmoji.FromName(ctx.Client, ":white_circle:").Id,
                        });
                        break;
                }
            }

            DiscordButtonComponent accpetBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"color_poll_confirm_{unique_id}", "Confirm", false);
            DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"color_poll_cancel_{unique_id}", "Cancel", false);

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Here is what the poll will look like. If everything looks good, hit 'Confirm' and you will be charged {settings.PollSubmittedPenalty} beer for the poll submission. Clicking cancel will *NOT* submit the poll and you will be forced to start over.")
                .AddEmbed(GeneratePollEmbed(ctx, question, pollOptions, PollType.Color))
                .AddComponents(cancelBtn, accpetBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == $"color_poll_confirm_{unique_id}")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Submitting color poll.."));
                    var sendPoll = await this.SendPendingPoll(ctx, question, pollOptions, PollType.Color);
                    if (sendPoll.Success)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll Created."));
                        return;
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Poll Creation Failed. {sendPoll.Message}"));
                        return;
                    }
                }
                // if the cancel button was pressed
                else if (e.Id == $"color_poll_cancel_{unique_id}")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll not created."));
                    return;
                }
            };
            return;
        }

        #endregion Color

        #region Yes or No

        [ModalCommand("gen_yesno")]
        public async Task GenerateYesNoPoll(ModalContext ctx, string question)
        {
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
            DiscordButtonComponent accpetBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"yesno_poll_confirm_{unique_id}", "Confirm", false);
            DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"yesno_poll_cancel_{unique_id}", "Cancel", false);

            List<PollOptions> pollOptions =
            [
                new PollOptions
                {
                    Answer = "Yes",
                    EmojiName = DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.Yes).Name,
                    EmojiId = DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.Yes).Id,
                },
                new PollOptions
                {
                    Answer = "No",
                    EmojiName = DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.No).Name,
                    EmojiId = DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.No).Id,
                },
            ];

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Here is what the poll will look like. If everything looks good, hit 'Confirm' and you will be charged {settings.PollSubmittedPenalty} beer for the poll submission. Clicking cancel will *NOT* submit the poll and you will be forced to start over.")
                .AddEmbed(GeneratePollEmbed(ctx, question, pollOptions, PollType.YesOrNo))
                .AddComponents(cancelBtn, accpetBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == $"yesno_poll_confirm_{unique_id}")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Posting poll.."));

                    var sendPoll = await SendPendingPoll(ctx, question, pollOptions, PollType.YesOrNo);
                    if (sendPoll.Success)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll Created."));
                        return;
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Poll Creation Failed. {sendPoll.Message}"));
                        return;
                    }
                }
                // if the cancel button was pressed
                else if (e.Id == $"yesno_poll_cancel_{unique_id}")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll not created."));
                    return;
                }
            };
            return;
        }

        #endregion Yes or No

        #region This Or That

        [ModalCommand("gen_thisorthat")]
        public async Task GenerateThisOrThatPoll(ModalContext ctx, string question, string thisResponse, string thatResponse)
        {
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
            DiscordButtonComponent accpetBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"thisorthat_poll_confirm_{unique_id}", "Confirm", false);
            DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"thisorthat_poll_cancel_{unique_id}", "Cancel", false);

            List<PollOptions> pollOptions =
            [
                new PollOptions
                {
                    Answer = thisResponse,
                    EmojiName = ":point_left:",
                    EmojiId = DiscordEmoji.FromName(ctx.Client, ":point_left:").Id,
                },
                new PollOptions
                {
                    Answer = thatResponse,
                    EmojiName = ":point_right:",
                    EmojiId = DiscordEmoji.FromName(ctx.Client, ":point_right:").Id,
                },
            ];

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Here is what the poll will look like. If everything looks good, hit 'Confirm' and you will be charged {settings.PollSubmittedPenalty} beer for the poll submission. Clicking cancel will *NOT* submit the poll and you will be forced to start over.")
                .AddEmbed(GeneratePollEmbed(ctx, question, pollOptions, PollType.ThisOrThat))
                .AddComponents(cancelBtn, accpetBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == $"thisorthat_poll_confirm_{unique_id}")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Posting poll.."));
                    var sendPoll = await SendPendingPoll(ctx, question, pollOptions, PollType.ThisOrThat);
                    if (sendPoll.Success)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll Created."));
                        return;
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Poll Creation Failed. {sendPoll.Message}"));
                        return;
                    }
                }
                // if the cancel button was pressed
                else if (e.Id == $"thisorthat_poll_cancel_{unique_id}")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll not created."));
                    return;
                }
            };
            return;
        }

        #endregion This Or That

        #region Hot Take

        [ModalCommand("gen_hottake")]
        public async Task GenerateHotTakePoll(ModalContext ctx, string question)
        {
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
            DiscordButtonComponent accpetBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"hottake_poll_confirm_{unique_id}", "Confirm", false);
            DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"hottake_poll_cancel_{unique_id}", "Cancel", false);

            List<PollOptions> pollOptions = new List<PollOptions>();
            pollOptions.Add(new PollOptions
            {
                Answer = "Agree",
                EmojiName = ":fire:",
                EmojiId = DiscordEmoji.FromName(ctx.Client, ":fire:").Id,
            });
            pollOptions.Add(new PollOptions
            {
                Answer = "Shit Take",
                EmojiName = ":poop:",
                EmojiId = DiscordEmoji.FromName(ctx.Client, ":poop:").Id,
            });

            var tests = ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Here is what the poll will look like. If everything looks good, hit 'Confirm' and you will be charged {settings.PollSubmittedPenalty} beer for the poll submission. Clicking cancel will *NOT* submit the poll and you will be forced to start over.")
                .AddEmbed(GeneratePollEmbed(ctx, question, pollOptions, PollType.HotTake))
                .AddComponents(cancelBtn, accpetBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == $"hottake_poll_confirm_{unique_id}")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Submitting poll.."));
                    var sendPoll = await SendPendingPoll(ctx, question, pollOptions, PollType.HotTake);
                    if (sendPoll.Success)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll Submitted."));
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Poll Creation Failed. {sendPoll.Message}"));
                    }
                }
                // if the cancel button was pressed
                else if (e.Id == $"hottake_poll_cancel_{unique_id}")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll creation canceled"));
                    return;
                }
            };
        }

        #endregion Hot Take

        #region Send Poll to Pending Polls Channel

        private async Task<Result> SendPendingPoll(ModalContext ctx, string question, List<PollOptions> options, PollType pollType)
        {
            DiscordChannel pollChannel = ctx.Guild.GetChannel(Variables.Channels.Waterbear.PendingPolls);
            if (pollChannel == null)
            {
                return new Result(false, "Poll channel not found.", null);
            }

            try
            {
                // Send the message to the channel
                DiscordMessage pollMessage = await ctx.Client.SendMessageAsync(pollChannel, GeneratePollEmbed(ctx, question, options, pollType));

                if (pollMessage == null)
                {
                    return new Result(false, "Failed to send poll message.", null);
                }

                var pendingPollResult = await this.pollService.AddPoll(new Poll
                {
                    AccountId = ctx.User.Id,
                    MessageId = pollMessage.Id,
                    Question = question,
                    Type = pollType,
                    Status = PollStatus.Pending,
                    EvaluatedOn = null,
                    SubmittedOn = DateTime.UtcNow,
                });

                if (pendingPollResult.Success)
                {
                    try
                    {
                        if (pendingPollResult.Data != null)
                        {
                            var addPollOptionsResult = await this.pollService.AddPollOption(pendingPollResult.Data as Poll, options);
                            if (addPollOptionsResult.Success)
                            {
                                // Send approval reactions
                                await pollMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.Yes));
                                await pollMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.No));
                            }
                            else
                            {
                                return new Result(false, "Adding poll options failed.", null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return new Result(false, ex.Message, null);
                    }

                    return new Result(true, "Poll added to pending polls.", pendingPollResult.Data);
                }
                else
                {
                    return new Result(false, pendingPollResult.Message, null);
                }
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        #endregion Send Poll to Pending Polls Channel

        #region Generate Poll Embed

        private DiscordEmbed GeneratePollEmbed(ModalContext ctx, string question, List<PollOptions> pollOptions, PollType? pollType)
        {
            // Set base embed color to white.
            DiscordColor embedColor = new DiscordColor(PollEmbedColors.PendingPoll);

            // Set description to empty string.
            string description = string.Empty;
            foreach (PollOptions option in pollOptions)
            {
                // If built in emoji
                if (option.EmojiId == 0)
                {
                    description += $"{DiscordEmoji.FromName(ctx.Client, option.EmojiName)} **{option.Answer}**\n";
                }
                else if (option.EmojiId != 0 && option.EmojiId != null)
                {
                    // If custom emoji
                    description += $"{DiscordEmoji.FromGuildEmote(ctx.Client, option.EmojiId.Value)} **{option.Answer}**\n";
                }
            }

            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.InfoIcon).Url,
                    Text = $"{pollType}",
                },
                Color = embedColor,
                Timestamp = DateTime.UtcNow,
                Title = $"__{question}__",
                Description = description,
            };

            return pollEmbed;
        }

        #endregion Generate Poll Embed
    }
}