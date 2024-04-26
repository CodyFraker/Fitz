using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;
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

            DiscordMessage poll = await ctx.Channel.SendMessageAsync(embed: GeneratePollEmbed(ctx, question, embedMessage));

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

            DiscordMessage pollMessage = await ctx.Channel.SendMessageAsync(embed: GeneratePollEmbed(ctx, question, embedMessage));

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

        private DiscordEmbed GeneratePollEmbed(ModalContext ctx, string question, string embedMessage)
        {
            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.PollEmojis.InfoIcon).Url,
                    Text = "Vote using reactions",
                },
                Color = new DiscordColor(250, 250, 250),
                Timestamp = DateTime.UtcNow,
                Title = $"**{question}**",
                Description = embedMessage,
            };

            return pollEmbed;
        }
    }
}