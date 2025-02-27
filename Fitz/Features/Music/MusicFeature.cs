using DSharpPlus;
using DSharpPlus.SlashCommands;
using Fitz.Core.Services.Features;
using Fitz.Features.Music.Play.Discord;
using Fitz.Features.Music.Stop.Discord;
using Fitz.Variables;
using System.Threading.Tasks;

namespace Fitz.Features.Music
{
    public sealed class MusicFeature : Feature
    {
        private readonly DiscordClient _client;
        private readonly SlashCommandsExtension _slashCommands;
        private readonly PlayAdapter _playAdapter;
        private readonly StopAdapter _stopAdapter;

        public override string Name => "Music";
        public override string Description => "Play music through voice channels.";

        public MusicFeature(
            DiscordClient client,
            PlayAdapter playAdapter,
            StopAdapter stopAdapter)
        {
            _client = client;
            _slashCommands = client.GetSlashCommands();
            _playAdapter = playAdapter;
            _stopAdapter = stopAdapter;
        }

        public override Task Enable()
        {
            // Register slash commands
            _slashCommands.RegisterCommands<PlayAdapter>(Guilds.Waterbear);
            _slashCommands.RegisterCommands<StopAdapter>(Guilds.Waterbear);

            return Task.CompletedTask;
        }

        public override Task Disable()
        {
            // Unregister slash commands
            //_slashCommands.UnregisterCommands<PlayAdapter>(Guilds.Waterbear);
            //_slashCommands.UnregisterCommands<StopAdapter>(Guilds.Waterbear);

            return Task.CompletedTask;
        }
    }
}