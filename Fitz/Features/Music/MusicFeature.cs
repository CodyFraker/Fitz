using DSharpPlus;
using DSharpPlus.SlashCommands;
using Fitz.Core.Services.Features;
using Fitz.Features.Music.Commands;
using Fitz.Variables;
using Lavalink4NET;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Fitz.Features.Music
{
    public sealed class MusicFeature : Feature
    {
        private IServiceScopeFactory scopeFactory;
        private readonly DiscordClient dClient;
        private IAudioService audioService;
        private readonly SlashCommandsExtension slash;

        public MusicFeature(IServiceScopeFactory scopeFactory, DiscordClient dClient, IAudioService audioService)
        {
            this.scopeFactory = scopeFactory;
            this.audioService = audioService;
            this.slash = dClient.GetSlashCommands();
        }

        public override string Name => "Music";

        public override string Description => "Play music through voice channels.";

        public override Task Disable()
        {
            return base.Disable();
        }

        public override Task Enable()
        {
            this.slash.RegisterCommands<MusicSlashCommands>(Guilds.Waterbear);

            return base.Enable();
        }
    }
}