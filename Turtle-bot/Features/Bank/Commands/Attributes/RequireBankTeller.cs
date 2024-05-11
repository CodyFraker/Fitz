using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using Fitz.Variables;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands.Attributes
{
    public sealed class RequireBankTeller : SlashCheckBaseAttribute
    {
        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx) => Task.FromResult(ctx.User.Id == Users.Dodecuplet);
    }
}