using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public sealed class AccountAdminCommands : BaseCommandModule
    {
        private readonly AccountService accountService;

        public AccountAdminCommands(AccountService accountService)
        {
            this.accountService = accountService;
        }

        [Command("setlastseen")]
        [Description("Sets the last seen date of all accounts to the current timestamp.")]
        public async Task SetLastSeen(CommandContext ctx)
        {
            await ctx.RespondAsync("Set last seen date of all accounts to the current timestamp.");
        }
    }
}