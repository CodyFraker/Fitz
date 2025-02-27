using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Music.Stop.Domain;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Music.Stop.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class StopAdapter : ApplicationCommandModule
    {
        private readonly StopService _stopService;

        public StopAdapter(StopService stopService)
        {
            _stopService = stopService ?? throw new ArgumentNullException(nameof(stopService));
        }

        [SlashCommand("stop", "Stop music playback and disconnect from the voice channel")]
        [RequireAccount]
        public async Task StopCommand(InteractionContext ctx)
        {
            // Defer the response to give us time to process
            await ctx.DeferAsync();

            try
            {
                // Create the command
                var command = new StopCommand(
                    userId: ctx.User.Id,
                    guildId: ctx.Guild.Id);

                // Execute the command
                bool success = await _stopService.StopAsync(command);

                if (success)
                {
                    // Create an embed for the response
                    var embed = new DiscordEmbedBuilder()
                        .WithTitle("Music Stopped")
                        .WithDescription("Music playback has been stopped and the bot has disconnected from the voice channel.")
                        .WithColor(DiscordColor.Red)
                        .WithFooter($"Requested by {ctx.User.Username}")
                        .WithTimestamp(DateTime.Now);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent("There is no active music playback to stop."));
                }
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Error stopping music: {ex.Message}"));
            }
        }
    }
} 