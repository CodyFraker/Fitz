namespace Fitz.Commands
{
    using System.Threading.Tasks;
    using Fitz.Models;
    using Fitz.DB.Models;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using System;
    using System.Linq;

    [Group("poll")]
    [Aliases("polls")]
    public class Polls : BaseCommandModule
    {

        [Command("-open")]
        [Aliases("-o")]
        [Description("Run Open Pools")]
        public async Task OpenPoll(CommandContext ctx, [RemainingText]string message)
        {

            // The title of what the embed will be asking/saying
            string[] embedTitle = message.Split('|');


            // The options, sepperated by a comma
            string[] pollOptions = embedTitle[1].Split(',');

            // The message of the embed
            string embedMessage = string.Empty;


            for (int i = 0; i < pollOptions.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":one:")} **{pollOptions[i]}**\n";
                        break;

                    case 1:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":two:")} **{pollOptions[i]}**\n";
                        break;

                    case 2:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":three:")} **{pollOptions[i]}**\n";
                        break;

                    case 3:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":four:")} **{pollOptions[i]}**\n";
                        break;

                    case 4:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":five:")} **{pollOptions[i]}**\n";
                        break;

                    case 5:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":six:")} **{pollOptions[i]}**\n";
                        break;

                    case 6:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":seven:")} **{pollOptions[i]}**\n";
                        break;

                    case 7:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":eight:")} **{pollOptions[i]}**\n";
                        break;

                    case 8:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":nine:")} **{pollOptions[i]}**\n";
                        break;

                    case 9:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":keycap_ten:")} **{pollOptions[i]}**\n";
                        break;
                }
            }

            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.InfoIcon).Url,
                    Text = "Vote using reactions",
                },
                Color = new DiscordColor(250, 250, 250),
                Timestamp = DateTime.UtcNow,
                Title = $"**{embedTitle[0]}**",
                Description = embedMessage,
            };

            DiscordMessage reactionMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed);

            for (int i = 0; i < pollOptions.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":one:"));
                        break;

                    case 1:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":two:"));
                        break;

                    case 2:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":three:"));
                        break;

                    case 3:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":four:"));
                        break;

                    case 4:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":five:"));
                        break;

                    case 5:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":six:"));
                        break;

                    case 6:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":seven:"));
                        break;

                    case 7:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":eight:"));
                        break;

                    case 8:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":nine:"));
                        break;

                    case 9:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":keycap_ten:"));
                        break;
                }
            }
        }

        [Command("-open2")]
        [Aliases("-o2")]
        [Description("Run Open Pools")]
        public async Task OpenPollCircles(CommandContext ctx, [RemainingText]string message)
        {

            // The title of what the embed will be asking/saying
            string[] embedTitle = message.Split('|');


            // The options, sepperated by a comma
            string[] pollOptions = embedTitle[1].Split(',');

            // The message of the embed
            string embedMessage = string.Empty;


            for (int i = 0; i < pollOptions.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":blue_circle:")} **{pollOptions[i]}**\n";
                        break;

                    case 1:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":green_circle:")} **{pollOptions[i]}**\n";
                        break;

                    case 2:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":orange_circle:")} **{pollOptions[i]}**\n";
                        break;

                    case 3:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":purple_circle:")} **{pollOptions[i]}**\n";
                        break;

                    case 4:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":red_circle:")} **{pollOptions[i]}**\n";
                        break;

                    case 5:
                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":yellow_circle:")} **{pollOptions[i]}**\n";
                        break;
                }
            }

            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.InfoIcon).Url,
                    Text = "Vote using reactions",
                },
                Color = new DiscordColor(250, 250, 250),
                Timestamp = DateTime.UtcNow,
                Title = $"**{embedTitle[0]}**",
                Description = embedMessage,
            };

            DiscordMessage reactionMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed);

            for (int i = 0; i < pollOptions.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":blue_circle:"));
                        break;

                    case 1:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":green_circle:"));
                        break;

                    case 2:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":orange_circle:"));
                        break;

                    case 3:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":purple_circle:"));
                        break;

                    case 4:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":red_circle:"));
                        break;

                    case 5:
                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":yellow_circle:"));
                        break;
                }
            }
        }

        [Command("-closed")]
        [Aliases("-c")]
        public async Task ClosedPoll(CommandContext ctx, [RemainingText]string message)
        {


            string[] embedTitle = message.Split('|');
            string embedMessage = string.Empty;

            embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.Yes)} **Yes**\n";
            embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.No)} **No**\n";

            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.InfoIcon).Url,
                    Text = "Vote using reactions",
                },
                Color = new DiscordColor(250, 250, 250),
                Timestamp = DateTime.UtcNow,
                Title = $"**{embedTitle[0]}**",
                Description = embedMessage,
            };
            DiscordMessage reactionMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed);
            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.Yes));
            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.No));
        }

        [Command("-closed3")]
        [Aliases("-c3")]
        public async Task ClosedPollThreeOption(CommandContext ctx, [RemainingText]string message)
        {


            string[] embedTitle = message.Split('|');
            string embedMessage = string.Empty;

            embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.Yes)} **Yes**\n";
            embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.No)} **No**\n";
            embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":regional_indicator_m:")} **Maybe**\n";

            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.InfoIcon).Url,
                    Text = "Vote using reactions",
                },
                Color = new DiscordColor(250, 250, 250),
                Timestamp = DateTime.UtcNow,
                Title = $"**{embedTitle[0]}**",
                Description = embedMessage,
            };
            DiscordMessage reactionMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed);
            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.Yes));
            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.No));
            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":regional_indicator_m:"));
        }

        [Command("-closed2")]
        [Aliases("-c2")]
        public async Task ClosedPollSometimes(CommandContext ctx, [RemainingText] string message)
        {


            string[] embedTitle = message.Split('|');
            string embedMessage = string.Empty;

            embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.Yes)} **Yes**\n";
            embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.No)} **No**\n";
            embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":regional_indicator_s:")} **Sometimes**\n";

            DiscordEmbed pollEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.InfoIcon).Url,
                    Text = "Vote using reactions",
                },
                Color = new DiscordColor(250, 250, 250),
                Timestamp = DateTime.UtcNow,
                Title = $"**{embedTitle[0]}**",
                Description = embedMessage,
            };
            DiscordMessage reactionMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed);
            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.Yes));
            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.No));
            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":regional_indicator_s:"));
        }
    }
}
