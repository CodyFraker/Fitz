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

        [Command("ping")]
        [Description("This command is to be used when you think the Fitz is frozen or stuck. It'll reply with **pong**")]
        public Task PingPongAsync(CommandContext ctx) => ctx.RespondAsync($"pong! Latency: {ctx.Client.Ping}ms");

        private static string GetUptime()
                => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss", CultureInfo.InvariantCulture);

        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
    }
}