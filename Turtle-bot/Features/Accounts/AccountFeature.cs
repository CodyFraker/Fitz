using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Variables;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    public class UserAccountFeature : Feature
    {
        private readonly JobManager jobManager;
        private readonly AccountJob accountJob;
        private readonly BotLog botLog;
        private readonly SlashCommandsExtension slash;
        private readonly CommandsNextExtension cNext;
        private AccountService accountService;

        public UserAccountFeature(DiscordClient dClient, AccountService accountService, BankService bankService, JobManager jobManager, BotLog botLog)
        {
            this.accountService = accountService;
            this.slash = dClient.GetSlashCommands();
            this.cNext = dClient.GetCommandsNext();
            this.jobManager = jobManager;
            this.botLog = botLog;
            this.accountJob = new AccountJob(accountService, dClient, botLog);
        }

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
            this.slash.RegisterCommands<AccountSlashCommands>(Guilds.DodeDuke);
            this.slash.RegisterCommands<AccountSlashCommands>(Guilds.Waterbear);
            this.slash.RegisterCommands<AccountAdminSlashCommands>();

            // Check to see if Fitz has an account registered in the database.
            if (accountService.FindAccount(Users.Fitz) == null)
            {
                accountService.CreateAccount(new Account
                {
                    Id = Users.Fitz,
                    CreatedDate = DateTime.UtcNow,
                    Username = "Fitz",
                    Beer = 128,
                    LifetimeBeer = 128,
                    Favorability = 100,
                });
            }

            return base.Enable();
        }
    }
}