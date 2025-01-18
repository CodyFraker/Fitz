using DSharpPlus.SlashCommands;
using Fitz.Variables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fitz.Features.AccountsRework.Create.Discord.Attributes
{
    public sealed class CreateAccountACL : SlashCheckBaseAttribute
    {
        private List<ulong> AllowedGuilds =
        [
            Guilds.DodeDuke,
            Guilds.Waterbear,
        ];

        private List<ulong> UserExceptions =
        [
            Users.Dodecuplet,
            Users.Spy,
        ];

        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            // Admins can use the command anywhere.
            // Users can run the command if the channel is a DM.
            if (UserExceptions.Contains(ctx.User.Id) || ctx.Channel.IsPrivate || AllowedGuilds.Contains(ctx.Guild.Id))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}