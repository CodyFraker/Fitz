using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Fitz.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Commands.Attributes
{
    public class RequireAccountAdmin : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.User.Id == Users.Dodecuplet);
    }
}