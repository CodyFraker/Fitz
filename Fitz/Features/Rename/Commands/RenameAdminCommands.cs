using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fitz.Features.Accounts;
using System.Threading.Tasks;

namespace Fitz.Features.Rename.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class RenameAdminCommands(RenameService renameService, AccountService accountService) : BaseCommandModule
    {
        private readonly RenameService renameService = renameService;
        private readonly AccountService accountService = accountService;

        [Command("renames")]
        [Description("Rename a user.")]
        public Task GetCurretRenames(CommandContext ctx)
        {
            this.renameService.GetExpiredRenames();
            return ctx.RespondAsync("Getting expired renames.");
        }
    }
}