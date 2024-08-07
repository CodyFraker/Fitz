﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts;
using Fitz.Features.Rename.Commands;
using Fitz.Features.Rename.Jobs;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public class RenameFeature(DiscordClient dClient, JobManager jobManager, RenameService renameService, AccountService accountService, BotLog botLog) : Feature
    {
        private readonly SlashCommandsExtension slash = dClient.GetSlashCommands();
        private readonly CommandsNextExtension cNext = dClient.GetCommandsNext();
        private readonly CheckForExpiredRenames renameJob = new CheckForExpiredRenames(dClient, renameService, accountService, botLog);
        private readonly CheckForNicknames checkForNicknames = new CheckForNicknames(dClient, renameService, accountService, botLog);
        private readonly JobManager jobManager = jobManager;

        public override string Name => "User Renaming";

        public override string Description => "Users can use their beer to rename other users within the guild.";

        public override Task Disable()
        {
            this.jobManager.RemoveJob(this.renameJob);
            this.jobManager.RemoveJob(this.checkForNicknames);
            this.cNext.UnregisterCommands<RenameAdminCommands>();
            return base.Disable();
        }

        public override Task Enable()
        {
            this.jobManager.AddJob(this.renameJob);
            this.jobManager.AddJob(this.checkForNicknames);
            this.slash.RegisterCommands<RenameSlashCommands>();
            this.cNext.RegisterCommands<RenameAdminCommands>();
            return base.Enable();
        }
    }
}