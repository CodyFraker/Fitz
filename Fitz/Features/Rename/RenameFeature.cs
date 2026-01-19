using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
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
            RecurringJob.RemoveIfExists("CheckForExpiredRenames");
            RecurringJob.RemoveIfExists("CheckForNicknames");
            return base.Disable();
        }

        public override Task Enable()
        {
            RecurringJob.AddOrUpdate("CheckForExpiredRenames", () => this.renameJob.Execute(), this.renameJob.Interval);
            RecurringJob.AddOrUpdate("CheckForNicknames", () => this.checkForNicknames.Execute(), this.checkForNicknames.Interval);
            this.slash.RegisterCommands<RenameSlashCommands>();
            return base.Enable();
        }
    }
}