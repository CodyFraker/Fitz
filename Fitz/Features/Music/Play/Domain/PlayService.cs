using Fitz.Core.Discord;
using Fitz.Core.Logging;
using Fitz.Variables.Channels;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Fitz.Features.Music.Play.Domain
{
    /// <summary>
    /// Service for playing music in a voice channel
    /// </summary>
    public class PlayService
    {
        private readonly IAudioService _audioService;

        public PlayService(IAudioService audioService)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }

        /// <summary>
        /// Plays a song in a voice channel
        /// </summary>
        /// <param name="command">The command containing the song and channel information</param>
        /// <returns>Information about the track that was played</returns>
        public async Task<LavalinkTrack> PlayAsync(PlayCommand command)
        {
            try
            {
                // Get or create a player for the guild
                var options = new QueuedLavalinkPlayerOptions
                {
                    // Configure player options as needed
                    SelfDeaf = true,
                };

                var player = await _audioService.Players.JoinAsync(
                    command.GuildId, 
                    command.VoiceChannelId, 
                    PlayerFactory.Queued, 
                    Options.Create(options));

                if (player == null)
                {
                    throw new InvalidOperationException("Failed to create audio player");
                }

                // Load the track
                var track = await _audioService.Tracks.LoadTrackAsync(command.Query, Lavalink4NET.Rest.Entities.Tracks.TrackSearchMode.YouTube);

                if (track != null)
                {
                    // Play the track
                    await player.PlayAsync(track);
                    return track;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}