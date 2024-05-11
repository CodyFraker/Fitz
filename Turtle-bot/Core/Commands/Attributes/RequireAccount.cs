using DSharpPlus.SlashCommands;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Fitz.Core.Commands.Attributes
{
    public sealed class RequireAccount : SlashCheckBaseAttribute
    {
        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            AccountService accountService = ctx.Services.GetService<AccountService>();

            Account account = accountService.FindAccount(ctx.User.Id);

            if (account != null)
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}