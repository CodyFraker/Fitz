namespace Fitz.Features.Music.Stop.Domain
{
    /// <summary>
    /// Command to stop music playback in a voice channel
    /// </summary>
    public class StopCommand
    {
        /// <summary>
        /// The Discord user ID requesting to stop music
        /// </summary>
        public ulong UserId { get; }

        /// <summary>
        /// The Discord guild ID where the music should be stopped
        /// </summary>
        public ulong GuildId { get; }

        public StopCommand(ulong userId, ulong guildId)
        {
            UserId = userId;
            GuildId = guildId;
        }
    }
} 