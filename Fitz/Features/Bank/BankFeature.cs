using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Fitz.Core.Services.Features;
using Fitz.Features.Accounts;
using Fitz.Features.Bank.Commands;
using Fitz.Variables;
using System.Threading.Tasks;

namespace Fitz.Features.Bank
{
    public class UserBankFeature : Feature
    {
        private readonly CommandsNextExtension cNext;
        private readonly SlashCommandsExtension slash;
        private readonly AccountService accountService;
        private readonly BankService bankService;
        private readonly DiscordClient dClient;

        public UserBankFeature(DiscordClient dClient, AccountService accountService, BankService bankService)
        {
            this.dClient = dClient;
            this.accountService = accountService;
            this.bankService = bankService;
            this.cNext = dClient.GetCommandsNext();
            this.slash = dClient.GetSlashCommands();
        }

        public override string Name => "Bank";

        public override string Description => "Follow the money, or beer.";

        public override Task Disable()
        {
            //this.cNext.UnregisterCommands<BankSlashCommands>();
            return base.Disable();
        }

        public override Task Enable()
        {
            this.slash.RegisterCommands<BankSlashCommands>();
            return base.Enable();
        }
    }
}