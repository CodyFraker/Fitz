using Fitz.Features.Music.Play.Domain;
using Fitz.Features.Music.Stop.Domain;
using Lavalink4NET.Tracks;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Music
{
    /// <summary>
    /// Service for managing music playback
    /// This class provides a facade for the domain services
    /// </summary>
    public class MusicService
    {
        private readonly PlayService _playService;
        private readonly StopService _stopService;

        public MusicService(PlayService playService, StopService stopService)
        {
            _playService = playService ?? throw new ArgumentNullException(nameof(playService));
            _stopService = stopService ?? throw new ArgumentNullException(nameof(stopService));
        }

        /// <summary>
        /// Plays a song in a voice channel
        /// </summary>
        /// <param name="userId">The user ID requesting to play music</param>
        /// <param name="guildId">The guild ID where the music should be played</param>
        /// <param name="voiceChannelId">The voice channel ID where the music should be played</param>
        /// <param name="query">The search query or URL for the song to play</param>
        /// <returns>Information about the track that was played</returns>
        public async Task<LavalinkTrack> PlayAsync(ulong userId, ulong guildId, ulong voiceChannelId, string query)
        {
            var command = new PlayCommand(userId, guildId, voiceChannelId, query);
            return await _playService.PlayAsync(command);
        }

        /// <summary>
        /// Stops music playback in a voice channel
        /// </summary>
        /// <param name="userId">The user ID requesting to stop music</param>
        /// <param name="guildId">The guild ID where the music should be stopped</param>
        /// <returns>True if playback was stopped, false otherwise</returns>
        public async Task<bool> StopAsync(ulong userId, ulong guildId)
        {
            var command = new StopCommand(userId, guildId);
            return await _stopService.StopAsync(command);
        }
    }
}