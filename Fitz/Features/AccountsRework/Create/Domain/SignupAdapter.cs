using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.AccountsRework.Create.Domain
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class AccountSlashCommands : ApplicationCommandModule
    {
    }
}