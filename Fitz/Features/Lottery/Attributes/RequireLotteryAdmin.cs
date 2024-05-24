using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fitz.Variables;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Attributes
{
    public sealed class RequireLotteryAdmin : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.User.Id == Users.Dodecuplet);
    }
}