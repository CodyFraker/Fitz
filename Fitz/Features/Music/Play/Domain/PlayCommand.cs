using System;

namespace Fitz.Features.Music.Play.Domain
{
    /// <summary>
    /// Command to play music in a voice channel
    /// </summary>
    public class PlayCommand
    {
        /// <summary>
        /// The Discord user ID requesting to play music
        /// </summary>
        public ulong UserId { get; }

        /// <summary>
        /// The Discord guild ID where the music should be played
        /// </summary>
        public ulong GuildId { get; }

        /// <summary>
        /// The Discord voice channel ID where the music should be played
        /// </summary>
        public ulong VoiceChannelId { get; }

        /// <summary>
        /// The search query or URL for the song to play
        /// </summary>
        public string Query { get; }

        public PlayCommand(ulong userId, ulong guildId, ulong voiceChannelId, string query)
        {
            UserId = userId;
            GuildId = guildId;
            VoiceChannelId = voiceChannelId;
            Query = query ?? throw new ArgumentNullException(nameof(query));
        }
    }
} 