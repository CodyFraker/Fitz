using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts;
using Fitz.Features.Rename.Commands;
using Fitz.Features.Rename.Jobs;
using Hangfire;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public class RenameFeature(DiscordClient dClient, RenameService renameService, AccountService accountService, BotLog botLog) : Feature
    {
        private readonly SlashCommandsExtension slash = dClient.GetSlashCommands();
        private readonly CommandsNextExtension cNext = dClient.GetCommandsNext();
        private readonly CheckForExpiredRenames renameJob = new CheckForExpiredRenames(dClient, renameService, accountService, botLog);
        private readonly CheckForNicknames checkForNicknames = new CheckForNicknames(dClient, renameService, accountService, botLog);

        public override string Name => "User Renaming";

        public override string Description => "Users can use their beer to rename other users within the guild.";

        public override Task Disable()
        {
            this.cNext.UnregisterCommands<RenameAdminCommands>();
            RecurringJob.RemoveIfExists("renameJob");
            RecurringJob.RemoveIfExists("CheckForNicknames");
            return base.Disable();
        }

        public override Task Enable()
        {
            this.slash.RegisterCommands<RenameSlashCommands>();
            this.cNext.RegisterCommands<RenameAdminCommands>();
            RecurringJob.AddOrUpdate("renameJob", () => this.renameJob.Execute(), this.renameJob.Interval);
            RecurringJob.AddOrUpdate("CheckForNicknames", () => this.checkForNicknames.Execute(), this.checkForNicknames.Interval);
            return base.Enable();
        }
    }
}