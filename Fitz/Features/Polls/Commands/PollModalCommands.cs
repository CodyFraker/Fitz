using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;
using Fitz.Core.Models;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Polls
{
    public class PollModalCommands : ModalCommandModule
    {
        private PollService pollService;

        public PollModalCommands(PollService pollService)
        {
            this.pollService = pollService;
        }

        #region Number

        [ModalCommand("gen_number")]
        public async Task GenerateNumberPoll(ModalContext ctx, string question, string choices)
        {
            // The message of the embed
            string embedMessage = string.Empty;
            string[] pollOptions = choices.Split(',');
            List<DiscordEmoji> options = new List<DiscordEmoji>();

            if (pollOptions.Length > 10 || pollOptions.Length <= 1)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"You need at least 2 options but no more than 10. You provided {pollOptions.Length} option(s).")
                    .AsEphemeral(true));
                return;
            }

            for (int i = 0; i < pollOptions.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":one:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":one:"));
                        break;

                    case 1:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":two:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":two:"));
                        break;

                    case 2:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":three:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":three:"));
                        break;

                    case 3:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":four:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":four:"));
                        break;

                    case 4:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":five:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":five:"));
                        break;

                    case 5:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":six:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":six:"));
                        break;

                    case 6:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":seven:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":seven:"));
                        break;

                    case 7:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":eight:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":eight:"));
                        break;

                    case 8:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":nine:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":nine:"));
                        break;

                    case 9:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":keycap_ten:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":keycap_ten:"));
                        break;
                }
            }

            DiscordButtonComponent accpetBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "number_poll_confirm", "Confirm", false);
            DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "number_poll_cancel", "Cancel", false);

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Here is what the poll will look like. Do you wish to post it?")
                .AddEmbed(GeneratePollEmbed(ctx, question, embedMessage, PollType.Number))
                .AddComponents(cancelBtn, accpetBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == "number_poll_confirm")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Posting poll.."));
                    var sendPoll = await SendPollMessageAsync(ctx, question, embedMessage, options, PollType.Number);
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
                else if (e.Id == "number_poll_cancel")
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
            // The message of the embed
            string embedMessage = string.Empty;
            string[] pollOptions = choices.Split(',');
            List<DiscordEmoji> options = new List<DiscordEmoji>();

            if (pollOptions.Length > 9)
            {
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"You can only have a maximum of 9 options for color polls. You provided {pollOptions.Length}")
                    .AsEphemeral(true));
                return;
            }

            for (int i = 0; i < pollOptions.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":blue_circle:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":blue_circle:"));
                        break;

                    case 1:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":green_circle:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":green_circle:"));
                        break;

                    case 2:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":orange_circle:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":orange_circle:"));
                        break;

                    case 3:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":purple_circle:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":purple_circle:"));
                        break;

                    case 4:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":red_circle:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":red_circle:"));
                        break;

                    case 5:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":yellow_circle:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":yellow_circle:"));
                        break;

                    case 6:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":brown_circle:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":brown_circle:"));
                        break;

                    case 7:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":black_circle:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":black_circle:"));
                        break;

                    case 8:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":white_circle:")} **{pollOptions[i]}**\n";
                        options.Add(DiscordEmoji.FromName(ctx.Client, ":white_circle:"));
                        break;
                }
            }

            DiscordButtonComponent accpetBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "color_poll_confirm", "Confirm", false);
            DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "color_poll_cancel", "Cancel", false);

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Here is what the poll will look like. Do you wish to post it?")
                .AddEmbed(GeneratePollEmbed(ctx, question, embedMessage, PollType.Color))
                .AddComponents(cancelBtn, accpetBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == "color_poll_confirm")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Posting poll.."));
                    var sendPoll = await SendPollMessageAsync(ctx, question, embedMessage, options, PollType.Color);
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
                else if (e.Id == "color_poll_cancel")
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
            // The message of the embed
            string embedMessage = string.Empty;

            embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.Yes)} **Yes**\n";
            embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.No)} **No**\n";

            DiscordButtonComponent accpetBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "yesno_poll_confirm", "Confirm", false);
            DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "yesno_poll_cancel", "Cancel", false);

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Here is what the poll will look like. Do you wish to post it?")
                .AddEmbed(GeneratePollEmbed(ctx, question, embedMessage, PollType.YesOrNo))
                .AddComponents(cancelBtn, accpetBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == "yesno_poll_confirm")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Posting poll.."));
                    List<DiscordEmoji> options = new List<DiscordEmoji>
                    {
                        DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.Yes),
                        DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.No),
                    };
                    var sendPoll = await SendPollMessageAsync(ctx, question, embedMessage, options, PollType.YesOrNo);
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
                else if (e.Id == "yesno_poll_cancel")
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
            // The message of the embed
            string embedMessage = string.Empty;

            embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":point_left:")} **{thisResponse}**\n";
            embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":point_right:")} **{thatResponse}**\n";

            DiscordButtonComponent accpetBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "thisorthat_poll_confirm", "Confirm", false);
            DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "thisorthat_poll_cancel", "Cancel", false);

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Here is what the poll will look like. Do you wish to post it?")
                .AddEmbed(GeneratePollEmbed(ctx, question, embedMessage, PollType.ThisOrThat))
                .AddComponents(cancelBtn, accpetBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == "thisorthat_poll_confirm")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Posting poll.."));
                    List<DiscordEmoji> options = new List<DiscordEmoji>
                    {
                        DiscordEmoji.FromName(ctx.Client, ":point_left:"),
                        DiscordEmoji.FromName(ctx.Client, ":point_right:"),
                    };
                    var sendPoll = await SendPollMessageAsync(ctx, question, embedMessage, options, PollType.ThisOrThat);
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
                else if (e.Id == "thisorthat_poll_cancel")
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
            // The message of the embed
            string embedMessage = string.Empty;

            embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":fire:")} **Agree**\n";
            embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":x:")} **Disagree**\n";

            DiscordButtonComponent accpetBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "hottake_poll_confirm", "Confirm", false);
            DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "hottake_poll_cancel", "Cancel", false);

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Here is what the poll will look like. Do you wish to post it?")
                .AddEmbed(GeneratePollEmbed(ctx, question, embedMessage, PollType.HotTake))
                .AddComponents(cancelBtn, accpetBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == "hottake_poll_confirm")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Posting poll.."));
                    List<DiscordEmoji> options = new List<DiscordEmoji>
                    {
                        DiscordEmoji.FromName(ctx.Client, ":fire:"),
                        DiscordEmoji.FromName(ctx.Client, ":x:"),
                    };
                    var sendPoll = await SendPollMessageAsync(ctx, question, embedMessage, options, PollType.HotTake);
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
                else if (e.Id == "hottake_poll_cancel")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll not created."));
                    return;
                }
            };
        }

        #endregion Hot Take

        private async Task<Result> SendPollMessageAsync(ModalContext ctx, string question, string embedMessage, List<DiscordEmoji> options, PollType pollType)
        {
            DiscordChannel pollChannel = ctx.Guild.GetChannel(Variables.Channels.Waterbear.Polls);
            if (pollChannel == null)
            {
                return new Result(false, "Poll channel not found.", null);
            }

            try
            {
                // Send the message to the channel
                DiscordMessage pollMessage = await ctx.Client.SendMessageAsync(pollChannel, embed: GeneratePollEmbed(ctx, question, embedMessage, pollType));

                // Iterate through the options and add reactions to the message
                foreach (DiscordEmoji option in options)
                {
                    await pollMessage.CreateReactionAsync(option);
                }

                Poll poll = await this.pollService.AddPoll(new Poll
                {
                    Question = question,
                    Type = pollType,
                    MessageId = pollMessage.Id,
                    Timestamp = DateTime.UtcNow,
                });

                await this.pollService.AddPollOption(poll, options);
                return new Result(true, "Poll created.", poll);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }

        private DiscordEmbed GeneratePollEmbed(ModalContext ctx, string question, string embedMessage, PollType? pollType)
        {
            DiscordColor embedColor = new DiscordColor(250, 250, 250);

            switch (pollType)
            {
                case PollType.Number:

                    break;

                case PollType.YesOrNo:
                    break;

                case PollType.ThisOrThat:
                    embedColor = new DiscordColor(225, 173, 1);
                    break;

                case PollType.HotTake:
                    embedColor = new DiscordColor(255, 103, 0);
                    break;
            }

            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.InfoIcon).Url,
                    Text = $"Vote using reactions | {pollType.ToString()}",
                },
                Color = embedColor,
                Timestamp = DateTime.UtcNow,
                Title = $"**{question}**",
                Description = embedMessage,
            };

            return pollEmbed;
        }
    }
}