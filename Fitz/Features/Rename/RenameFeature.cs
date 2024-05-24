using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts;
using Fitz.Features.Rename.Commands;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public class RenameFeature : Feature
    {
        private readonly DiscordClient dClient;
        private readonly SlashCommandsExtension slash;
        private readonly CommandsNextExtension cNext;
        private readonly RenameJob renameJob;
        private readonly JobManager jobManager;
        private readonly RenameService renameService;
        private readonly AccountService accountService;

        public RenameFeature(DiscordClient dClient, JobManager jobManager)
        {
            this.slash = dClient.GetSlashCommands();
            this.renameJob = new RenameJob(dClient, renameService, accountService);
            this.jobManager = jobManager;
            this.cNext = dClient.GetCommandsNext();
        }

        public override string Name => "User Renaming";

        public override string Description => "Users can use their beer to rename other users within the guild.";

        public override Task Disable()
        {
            this.jobManager.RemoveJob(this.renameJob);
            this.cNext.UnregisterCommands<RenameAdminCommands>();
            return base.Disable();
        }

        public override Task Enable()
        {
            this.jobManager.AddJob(this.renameJob);
            this.slash.RegisterCommands<RenameSlashCommands>();
            this.cNext.RegisterCommands<RenameAdminCommands>();
            return base.Enable();
        }
    }
}