using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts.Commands;
using Fitz.Variables;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    public class UserAccountFeature(DiscordClient dClient, AccountService accountService, JobManager jobManager, BotLog botLog) : Feature
    {
        private readonly JobManager jobManager = jobManager;
        private readonly AccountJob accountJob = new AccountJob(accountService, dClient, botLog);
        private readonly SlashCommandsExtension slash = dClient.GetSlashCommands();
        private readonly CommandsNextExtension cNext = dClient.GetCommandsNext();
        private AccountService accountService = accountService;

        public override string Name => "Accounts";

        public override string Description => "Enable users to create accounts associated with the bot.";

        public override Task Disable()
        {
            this.jobManager.RemoveJob(this.accountJob);
            this.cNext.UnregisterCommands<AccountAdminSlashCommands>();
            return base.Disable();
        }

        public override Task Enable()
        {
            this.jobManager.AddJob(this.accountJob);

            // For some reason, discord isn't wanting to register the command globally.
            // Hence why I register the commands in two guilds here.
            this.slash.RegisterCommands<AccountSlashCommands>(Guilds.Waterbear);
            this.slash.RegisterCommands<AccountSlashCommands>(Guilds.DodeDuke);
            this.slash.RegisterCommands<AccountAdminSlashCommands>();

            // Check to see if Fitz has an account registered in the database.
            if (accountService.FindAccount(Users.Fitz) == null)
            {
                accountService.CreateFitzAccountAsync();
            }

            return base.Enable();
        }
    }
}