using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Fitz.Features.Music.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class MusicSlashCommands : ApplicationCommandModule
    {
        private IAudioService audioService;

        public MusicSlashCommands(IAudioService audioService)
        {
            this.audioService = audioService;
        }

        [SlashCommand("play", "Play a song at the cost of beer.")]
        public async Task Play(InteractionContext ctx, [Option("song", "The song to play.")] string song)
        {
            var player = await GetLavaLinkPlayer(ctx, true).ConfigureAwait(false);

            if (player == null)
            {
                return;
            }

            var loadOptions = new TrackLoadOptions
            {
                SearchMode = TrackSearchMode.YouTube,
            };

            var track = await audioService.Tracks.LoadTrackAsync(song, loadOptions).ConfigureAwait(false);

            await player.PlayAsync(song).ConfigureAwait(false);
        }

        [SlashCommand("stop", "Stop the current song.")]
        public async Task Stop(InteractionContext ctx)
        {
            var player = await GetLavaLinkPlayer(ctx, false).ConfigureAwait(false);

            if (player == null)
            {
                return;
            }
            if (player.CurrentTrack == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent(
                        "There is nothing playing.")
                    .AsEphemeral(true));
            }

            await player.StopAsync().ConfigureAwait(false);
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(
                    "Stopping..").AsEphemeral(true));
        }

        private async Task<QueuedLavalinkPlayer?> GetLavaLinkPlayer(InteractionContext ctx, bool connectToVoice = true)
        {
            var guildId = ctx.Guild.Id;
            var voiceChannelId = ctx.Member.VoiceState?.Channel?.Id;
            var channelBehavior = connectToVoice ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None;

            var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: channelBehavior);

            var options = new QueuedLavalinkPlayerOptions
            {
                SelfDeaf = true,
                SelfMute = false,
                DisconnectOnStop = false,
            };

            var optionsWrapper = Options.Create(options);

            var result = await audioService.Players
                .RetrieveAsync(guildId, voiceChannelId, PlayerFactory.Queued, optionsWrapper, retrieveOptions).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                var errorMessage = result.Status switch
                {
                    PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
                    PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
                    _ => "Unknown error.",
                };

                return null;
            }

            return result.Player;
        }
    }
}