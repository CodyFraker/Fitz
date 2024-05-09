using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Fitz.Features.Polls.Polls
{
    public class PollModalCommands : ModalCommandModule
    {
        private PollService pollService;

        public PollModalCommands(PollService pollService)
        {
            this.pollService = pollService;
        }

        [ModalCommand("generate_poll")]
        public async Task GeneratePoll(ModalContext ctx, string question, string responseOne, string responseTwo, string responseThree, string responseFour)
        {
            // The message of the embed
            string embedMessage = string.Empty;

            if (responseOne != null)
            {
                embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":blue_circle:")} **{responseOne}**\n";
            }
            if (responseTwo != null)
            {
                embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":green_circle:")} **{responseTwo}**\n";
            }
            if (responseThree != null)
            {
                embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":orange_circle:")} **{responseThree}**\n";
            }
            if (responseFour != string.Empty)
            {
                embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":purple_circle:")} **{responseFour}**\n";
            }

            DiscordMessage poll = await ctx.Channel.SendMessageAsync(embed: GeneratePollEmbed(ctx, question, embedMessage, PollType.Open));

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Created the poll.").AsEphemeral());

            if (responseOne != null)
            {
                await poll.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":blue_circle:"));
            }
            if (responseTwo != null)
            {
                await poll.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":green_circle:"));
            }
            if (responseThree != null)
            {
                await poll.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":orange_circle:"));
            }
            if (responseFour != null)
            {
                await poll.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":purple_circle:"));
            }
        }

        [ModalCommand("gen_yesno")]
        public async Task GenerateYesNoPoll(ModalContext ctx, string question)
        {
            // The message of the embed
            string embedMessage = string.Empty;

            embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.Yes)} **Yes**\n";
            embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.No)} **No**\n";

            DiscordMessage pollMessage = await ctx.Channel.SendMessageAsync(embed: GeneratePollEmbed(ctx, question, embedMessage, PollType.Closed));

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Created the poll.").AsEphemeral());

            await pollMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.Yes));
            await pollMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.No));

            List<DiscordEmoji> options = new List<DiscordEmoji>
            {
                DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.Yes),
                DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.No),
            };

            Poll poll = await this.pollService.AddPoll(new Poll
            {
                Question = question,
                Type = PollType.Closed,
                MessageId = pollMessage.Id,
                Timestamp = DateTime.UtcNow,
            });

            await this.pollService.AddPollOption(poll, options);
        }

        [ModalCommand("gen_thisorthat")]
        public async Task GenerateThisOrThatPoll(ModalContext ctx, string question, string thisResponse, string thatResponse)
        {
            // The message of the embed
            string embedMessage = string.Empty;

            embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":point_left:")} **{thisResponse}**\n";
            embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":point_right:")} **{thatResponse}**\n";

            DiscordMessage pollMessage = await ctx.Channel.SendMessageAsync(embed: GeneratePollEmbed(ctx, question, embedMessage, PollType.ThisOrThat));

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Created the poll.").AsEphemeral());

            await pollMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":point_left:"));
            await pollMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":point_right:"));

            List<DiscordEmoji> options = new List<DiscordEmoji>
            {
                DiscordEmoji.FromName(ctx.Client, ":point_left:"),
                DiscordEmoji.FromName(ctx.Client, ":point_right:"),
            };

            Poll poll = await this.pollService.AddPoll(new Poll
            {
                Question = question,
                Type = PollType.ThisOrThat,
                MessageId = pollMessage.Id,
                Timestamp = DateTime.UtcNow,
            });

            await this.pollService.AddPollOption(poll, options);
        }

        [ModalCommand("gen_hottake")]
        public async Task GenerateHotTakePoll(ModalContext ctx, string question, string thisResponse, string thatResponse)
        {
            // The message of the embed
            string embedMessage = string.Empty;

            embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":point_left:")} **Agree**\n";
            embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":point_right:")} **Disagree**\n";

            DiscordMessage pollMessage = await ctx.Channel.SendMessageAsync(embed: GeneratePollEmbed(ctx, question, embedMessage, PollType.ThisOrThat));

            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Created the poll.").AsEphemeral());

            await pollMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":point_left:"));
            await pollMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":point_right:"));

            List<DiscordEmoji> options = new List<DiscordEmoji>
            {
                DiscordEmoji.FromName(ctx.Client, ":point_left:"),
                DiscordEmoji.FromName(ctx.Client, ":point_right:"),
            };

            Poll poll = await this.pollService.AddPoll(new Poll
            {
                Question = question,
                Type = PollType.ThisOrThat,
                MessageId = pollMessage.Id,
                Timestamp = DateTime.UtcNow,
            });

            await this.pollService.AddPollOption(poll, options);
        }

        private DiscordEmbed GeneratePollEmbed(ModalContext ctx, string question, string embedMessage, PollType? pollType)
        {
            DiscordColor embedColor = new DiscordColor(250, 250, 250);

            switch (pollType)
            {
                case PollType.Open:

                    break;

                case PollType.Closed:
                    break;

                case PollType.ThisOrThat:

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
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = "https://img.icons8.com/?size=256&id=nYDdQi7K5MAP&format=png",
                },
                Title = $"**{question}**",
                Description = embedMessage,
            };

            return pollEmbed;
        }
    }
}