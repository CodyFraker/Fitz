using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Features.Bank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Rename.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class RenameSlashCommands : ApplicationCommandModule
    {
        private readonly RenameService renameService;
        private readonly BankService bankService;
        private const int RenameCost = 100;

        [SlashCommand("rename", "Rename a user within the guild.")]
        public async Task Rename(InteractionContext ctx,
            [Option("User", "Manage whose account?")] DiscordUser user = null,
            [Option("New Name", "What should their new name be?")] string newName = null)
        {
            // Check to see if a user was provided
            if (user == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to specify a user.").AsEphemeral(true));
                return;
            }

            // Check to see if a new name was provided
            if (string.IsNullOrWhiteSpace(newName))
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to specify a new name for that user.").AsEphemeral(true));
                return;
            }
        }
    }
}