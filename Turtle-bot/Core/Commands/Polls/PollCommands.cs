using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;
using System;
using System.Threading.Tasks;

namespace Fitz.Core.Commands.Polls
{
    public class PollCommands : ModalCommandModule
    {
        public PollCommands()
        {
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
            if (responseFour != String.Empty)
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

        private DiscordEmbed GeneratePollEmbed(ModalContext ctx, string question, string embedMessage)
        {
            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.InfoIcon).Url,
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