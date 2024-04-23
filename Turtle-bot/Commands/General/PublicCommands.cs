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

    /// <summary>
    /// This class pertains to the commands that can be ran practically anywhere. They're typically short and simple commands to run.
    /// </summary>
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class PublicCommands : BaseCommandModule
    {
        private readonly FitzContext db;

        public PublicCommands(FitzContext db)
        {
            this.db = db;
        }

        [Command("beer")]
        [Description("Give me a beer")]
        public async Task LTPAsync(CommandContext ctx)
        {
            this.db.Beer.Add(new Beers()
            {
                UserId = ctx.User.Id,
                GuildID = ctx.Guild.Id,
                Timestamp = DateTime.UtcNow,
            });
            await this.db.SaveChangesAsync().ConfigureAwait(false);
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":beer:")).ConfigureAwait(false);
        }

        //[Command("poll")]
        //[Description("Fitz will run a poll in a specific channel")]
        //public async Task SayAsync(CommandContext ctx, string pollType, [RemainingText]string message)
        //{
        //    string[] titleMessage = message.Split('|');
        //    string[] splitMessage = titleMessage[1].Split(',');

        //    string embedMessage = string.Empty;
        //    switch (pollType)
        //    {
        //        case "open":
        //            for (int i = 0; i < splitMessage.Length; i++)
        //            {
        //                switch (i)
        //                {
        //                    case 0:
        //                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":one:")} **{splitMessage[i]}**\n";
        //                        break;

        //                    case 1:
        //                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":two:")} **{splitMessage[i]}**\n";
        //                        break;

        //                    case 2:
        //                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":three:")} **{splitMessage[i]}**\n";
        //                        break;

        //                    case 3:
        //                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":four:")} **{splitMessage[i]}**\n";
        //                        break;

        //                    case 4:
        //                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":five:")} **{splitMessage[i]}**\n";
        //                        break;

        //                    case 5:
        //                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":six:")} **{splitMessage[i]}**\n";
        //                        break;

        //                    case 6:
        //                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":seven:")} **{splitMessage[i]}**\n";
        //                        break;

        //                    case 7:
        //                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":eight:")} **{splitMessage[i]}**\n";
        //                        break;

        //                    case 8:
        //                        embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":nine:")} **{splitMessage[i]}**\n";
        //                        break;
        //                }
        //            }
        //        break;

        //        case "closed":

        //            if(splitMessage.Length > 2)
        //            {
        //                await ctx.Channel.SendMessageAsync("You have more than two options for a closed ended question you damn idiot.");
        //                return;
        //            }
        //            else
        //            {
        //                embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.Yes)} **{splitMessage[0]}**\n";
        //                embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.No)} **{splitMessage[1]}**\n";
        //            }
        //            break;

        //        case "closed-3":

        //            if (splitMessage.Length > 2)
        //            {
        //                await ctx.Channel.SendMessageAsync("You have more than two options for a closed ended question you damn idiot.");
        //                return;
        //            }
        //            else
        //            {
        //                embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.Yes)} **{splitMessage[0]}**\n";
        //                embedMessage += $"{DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.No)} **{splitMessage[1]}**\n";
        //                embedMessage += $"{DiscordEmoji.FromName(ctx.Client, ":regional_indicator_m:")} **Maybe**\n";
        //            }
        //            break;
        //    }

        //    DiscordEmbed pollEmbed = new DiscordEmbedBuilder
        //    {
        //        Footer = new DiscordEmbedBuilder.EmbedFooter
        //        {
        //            IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.InfoIcon).Url,
        //            Text = "Vote using reactions",
        //        },
        //        Color = new DiscordColor(250, 250, 250),
        //        Timestamp = DateTime.UtcNow,
        //        Title = $"**{titleMessage[0]}**",
        //        Description = embedMessage,
        //    };

        //    DiscordMessage reactionMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed);

        //    switch (pollType)
        //    {
        //        case "closed-3":
        //            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.Yes));
        //            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.No));
        //            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":regional_indicator_m:"));
        //            break;
        //        case "closed":
        //            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.Yes));
        //            await reactionMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, Variables.Emojis.Polls.No));
        //            break;
        //        case "open":
        //            for (int i = 0; i < splitMessage.Length; i++)
        //            {
        //                switch (i)
        //                {
        //                    case 0:
        //                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":one:"));
        //                        break;

        //                    case 1:
        //                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":two:"));
        //                        break;

        //                    case 2:
        //                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":three:"));
        //                        break;

        //                    case 3:
        //                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":four:"));
        //                        break;

        //                    case 4:
        //                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":five:"));
        //                        break;

        //                    case 5:
        //                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":six:"));
        //                        break;

        //                    case 6:
        //                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":seven:"));
        //                        break;

        //                    case 7:
        //                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":eight:"));
        //                        break;

        //                    case 8:
        //                        await reactionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":nine:"));
        //                        break;
        //                }
        //            }
        //            break;
        //    }
        //}
    }
}