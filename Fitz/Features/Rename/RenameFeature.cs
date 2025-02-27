using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Rename.Commands;
using Fitz.Features.Rename.Jobs;
using Fitz.Features.Rename.Notify.Domain;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public class RenameFeature : Feature
    {
        private readonly SlashCommandsExtension slash;
        private readonly CommandsNextExtension cNext;
        private readonly CheckForExpiredRenames renameJob;
        private readonly CheckForNicknames checkForNicknames;
        private readonly NotifyRenamesJob notifyRenamesJob;
        private readonly JobManager jobManager;

        public RenameFeature(
            DiscordClient dClient, 
            JobManager jobManager, 
            RenameService renameService, 
            AccountService accountService, 
            NotifyRenameService notifyRenameService,
            BotLog botLog)
        {
            this.slash = dClient.GetSlashCommands();
            this.cNext = dClient.GetCommandsNext();
            this.renameJob = new CheckForExpiredRenames(dClient, renameService, accountService, botLog);
            this.checkForNicknames = new CheckForNicknames(dClient, renameService, accountService, botLog);
            this.notifyRenamesJob = new NotifyRenamesJob(dClient, notifyRenameService, botLog);
            this.jobManager = jobManager;
        }

        public override string Name => "User Renaming";

        public override string Description => "Users can use their beer to rename other users within the guild.";

        public override Task Disable()
        {
            this.jobManager.RemoveJob(this.renameJob);
            this.jobManager.RemoveJob(this.checkForNicknames);
            this.jobManager.RemoveJob(this.notifyRenamesJob);
            this.cNext.UnregisterCommands<RenameAdminCommands>();
            return base.Disable();
        }

        public override Task Enable()
        {
            this.jobManager.AddJob(this.renameJob);
            this.jobManager.AddJob(this.checkForNicknames);
            this.jobManager.AddJob(this.notifyRenamesJob);
            this.slash.RegisterCommands<RenameSlashCommands>();
            this.cNext.RegisterCommands<RenameAdminCommands>();
            return base.Enable();
        }
    }
}