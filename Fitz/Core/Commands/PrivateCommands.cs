namespace Fitz.Core.Commands
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Threading.Tasks;
    using Fitz.Variables;
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Fitz.Features.Accounts.Models;
    using Fitz.Variables.Emojis;

    public class PrivateCommands : BaseCommandModule
    {
        /// <summary>
        /// Sends the assembly version which can be found under Project -> {Project} Settings -> Package
        /// </summary>
        /// <param name="ctx">Command Context</param>
        /// <returns>Assembly Version through Discord</returns>
        [Command("build")]
        [Description("Provides current build bloon is on.")]
        public Task BuildAsync(CommandContext ctx) => ctx.RespondAsync($"{Assembly.GetEntryAssembly().GetName().Version}");

        /// <summary>
        /// Sends basic information such as total time the Fitz has been running and the library version this project is using to interface with Discord's API
        /// </summary>
        /// <param name="ctx">Command Context</param>
        /// <returns>Memory Heap size, Library version, and total uptime.</returns>
        [Command("info")]
        [Description("Displays basic info and statistics about Fitz and this discord server")]
        public Task Info(CommandContext ctx) => ctx.RespondAsync(
                $"{Formatter.Bold("Info")}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Library: Discord.Net ({ctx.Client.VersionString})\n" +

                // $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n");

        /// <summary>
        /// Make the Fitz say whatever you want in a particular channel
        /// </summary>
        /// <param name="ctx">Command context</param>
        /// <param name="channelID">The Discord channel ID</param>
        /// <param name="message">Whatever message you want.</param>
        /// <returns>Sends the desired message into that provided channel.</returns>
        [Command("say")]
        [Description("Make Fitz send a message in any channel.")]
        public async Task SayAsync(CommandContext ctx, ulong channelID, [RemainingText] string message)
        {
            DiscordChannel channel = await ctx.Client.GetChannelAsync(channelID).ConfigureAwait(false);
            await channel.SendMessageAsync(message).ConfigureAwait(false);
            await ctx.RespondAsync($"Sent {message} to channel: {channel.Name}").ConfigureAwait(false);
        }

        /// <summary>
        /// Make the Fitz say whatever you want in a particular channel
        /// </summary>
        /// <param name="ctx">Command context</param>
        /// <param name="channelID">The Discord channel ID</param>
        /// <param name="message">Whatever message you want.</param>
        /// <returns>Sends the desired message into that provided channel.</returns>
        [Command("rules")]
        [Description("Make Fitz send a message in any channel.")]
        public async Task HelpEmbed(CommandContext ctx)
        {
            DiscordEmbedBuilder helpEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.InfoIcon).Url,
                    Text = $"Rules & Help",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Description = "Hi. I'm [Fitz](https://12ozmouse.fandom.com/wiki/Mouse_Fitzgerald). I'm in charge of you now. In order to use [#general](https://discord.com/channels/196820438398140417/196820438398140417) and other channels. You're going to need to run `/signup`.\n" +
                "We drink a lot of beer around these parts. In fact I decided I will now only accept this as a form of currency. Being active in here means you get beer. I want beer. I dislike not having beer. You can give me a beer by doing `/beer`",
            };

            helpEmbed.AddField($"Lottery", $"I run a lottery where you can gamble your beer away. Head over to [#lottery](https://discord.com/channels/1022879771526451240/1232083050268069948) and run `/lottery` to get started.", false);

            helpEmbed.AddField($"Polls", $"I ask life's hardest questions over in [#polls](https://discord.com/channels/1022879771526451240/1066465880671780936). I will give you beer for your response.", false);

            helpEmbed.AddField($"Renaming", $"For a small fee, I will rename anyone in the discord to anything you wish for a duration you provide. `/rename` to get started. Each person must have an account with me.", false);

            helpEmbed.AddField($"Favorability", $"Determines how much I like you as a person. I don't care to be asked to do a bunch of things for people all the time. I do like beer, though. If I don't like you, I might just take your beer and not do what you told me. That's just how I roll, baby.", false);

            helpEmbed.AddField($"Happy Hour", $"Between 8PM-11:59PM EST, people in the voice channels will be awarded beer.", false);

            helpEmbed.AddField($"Future..", $"I'm still being worked on. There's many things I can do. Because that's something I can do.", false);

            DiscordChannel discordChannel = ctx.Channel;
            await discordChannel.SendMessageAsync(embed: helpEmbed);
        }

        [Command("ping")]
        [Description("This command is to be used when you think the Fitz is frozen or stuck. It'll reply with **pong**")]
        public Task PingPongAsync(CommandContext ctx) => ctx.RespondAsync($"pong! Latency: {ctx.Client.Ping}ms");

        private static string GetUptime()
                => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss", CultureInfo.InvariantCulture);

        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
    }
}