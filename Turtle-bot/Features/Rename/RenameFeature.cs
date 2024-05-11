using DSharpPlus;
using DSharpPlus.SlashCommands;
using Fitz.Core.Services.Features;
using Fitz.Features.Rename.Commands;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public class RenameFeature : Feature
    {
        private readonly DiscordClient dClient;
        private readonly SlashCommandsExtension slash;

        public RenameFeature(DiscordClient dClient)
        {
            this.slash = dClient.GetSlashCommands();
        }

        public override string Name => "User Renaming";

        public override string Description => "Users can use their beer to rename other users within the guild.";

        public override Task Disable()
        {
            return base.Disable();
        }

        public override Task Enable()
        {
            this.slash.RegisterCommands<RenameSlashCommands>();
            return base.Enable();
        }
    }
}