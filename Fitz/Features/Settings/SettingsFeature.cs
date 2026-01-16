using DSharpPlus;
using DSharpPlus.SlashCommands;
using Fitz.Core.Services.Features;
using Fitz.Features.Settings.Commands;
using System.Threading.Tasks;

namespace Fitz.Features.Settings
{
    public class SettingsFeature(DiscordClient dClient) : Feature
    {
        private readonly DiscordClient dClient = dClient;
        private readonly SlashCommandsExtension slash = dClient.GetSlashCommands();

        public override string Name => "Settings";

        public override string Description => "Manage bot settings.";

        public override Task Disable()
        {
            return base.Disable();
        }

        public override Task Enable()
        {
            this.slash.RegisterCommands<SettingsSlashCommands>();
            return base.Enable();
        }
    }
}
