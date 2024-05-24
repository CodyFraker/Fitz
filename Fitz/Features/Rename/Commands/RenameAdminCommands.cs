using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fitz.Features.Accounts;
using Fitz.Features.Rename.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Rename.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class RenameAdminCommands : BaseCommandModule
    {
        private readonly RenameService renameService;
        private readonly AccountService accountService;

        public RenameAdminCommands(RenameService renameService, AccountService accountService)
        {
            this.renameService = renameService;
            this.accountService = accountService;
        }

        [Command("renames")]
        [Description("Rename a user.")]
        public Task GetCurretRenames(CommandContext ctx)
        {
            this.renameService.GetExpiredRenames();
            return ctx.RespondAsync("Getting expired renames.");
        }
    }
}