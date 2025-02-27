using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Music.Play.Domain;
using Lavalink4NET.Tracks;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Music.Play.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class PlayAdapter : ApplicationCommandModule
    {
        private readonly PlayService _playService;

        public PlayAdapter(PlayService playService)
        {
            _playService = playService ?? throw new ArgumentNullException(nameof(playService));
        }

        [SlashCommand("play", "Play a song in your voice channel")]
        [RequireAccount]
        public async Task PlayCommand(InteractionContext ctx, 
            [Option("song", "The song to play (search query or URL)")] string song)
        {
            // Defer the response to give us time to process
            await ctx.DeferAsync();

            // Check if the user is in a voice channel
            var member = ctx.Member;
            var voiceState = member?.VoiceState;
            var voiceChannel = voiceState?.Channel;

            if (voiceChannel == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("You need to be in a voice channel to use this command."));
                return;
            }

            try
            {
                // Create the command
                var command = new PlayCommand(
                    userId: ctx.User.Id,
                    guildId: ctx.Guild.Id,
                    voiceChannelId: voiceChannel.Id,
                    query: song);

                // Execute the command
                LavalinkTrack track = await _playService.PlayAsync(command);

                if (track != null)
                {
                    // Create an embed with track information
                    var embed = new DiscordEmbedBuilder()
                        .WithTitle("Now Playing")
                        .WithDescription($"[{track.Title}]({track.Uri})")
                        .WithColor(DiscordColor.Purple)
                        .WithFooter($"Requested by {ctx.User.Username}")
                        .WithTimestamp(DateTime.Now);

                    if (!string.IsNullOrEmpty(track.ArtworkUri?.ToString()))
                    {
                        embed.WithThumbnail(track.ArtworkUri.ToString());
                    }

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"Could not find any tracks matching: {song}"));
                }
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Error playing music: {ex.Message}"));
            }
        }
    }
} 