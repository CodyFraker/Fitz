using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.Contexts;
using Fitz.Variables.Emojis;
using DSharpPlus.CommandsNext;
using DSharpPlus.ModalCommands;
using Fitz.Features.Accounts;

namespace Fitz.Core.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class GeneralSlashCommands : ApplicationCommandModule
    {
        private readonly BotContext db;
        private readonly AccountService accountService;

        public GeneralSlashCommands(BotContext db, AccountService accountService)
        {
            this.db = db;
            this.accountService = accountService;
        }
    }
}
