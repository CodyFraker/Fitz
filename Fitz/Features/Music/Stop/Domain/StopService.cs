using Fitz.Core.Discord;
using Fitz.Variables;
using Lavalink4NET;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Music.Stop.Domain
{
    /// <summary>
    /// Service for stopping music playback in a voice channel
    /// </summary>
    public class StopService
    {
        private readonly IAudioService _audioService;
        private readonly BotLog _botLog;

        public StopService(IAudioService audioService, BotLog botLog)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _botLog = botLog ?? throw new ArgumentNullException(nameof(botLog));
        }

        /// <summary>
        /// Stops music playback in a voice channel
        /// </summary>
        /// <param name="command">The command containing the guild information</param>
        /// <returns>True if playback was stopped, false otherwise</returns>
        public async Task<bool> StopAsync(StopCommand command)
        {
            try
            {
                // Get the player for the guild
                var player = await _audioService.Players.GetPlayerAsync(command.GuildId);

                if (player == null)
                {
                    _botLog.Information(LogConsoleSettings.Commands, FeatureEmojis.ToggleOff, $"No active player found for guild {command.GuildId}");
                    return false;
                }

                // Stop playback and disconnect
                await player.StopAsync();
                await player.DisconnectAsync();

                _botLog.Information(LogConsoleSettings.Commands, FeatureEmojis.ToggleOff, $"Stopped music playback for guild {command.GuildId} by user {command.UserId}");
                return true;
            }
            catch (Exception ex)
            {
                _botLog.Error($"Error stopping music: {ex.Message}");
                throw;
            }
        }
    }
}