using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fitz.Core.Contexts;
using Fitz.Variables.Emojis;
using DSharpPlus.CommandsNext;
using DSharpPlus.ModalCommands;
using Fitz.Features.Accounts;

namespace Fitz.Core.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class GeneralSlashCommands : ApplicationCommandModule
    {
        private readonly BotContext db;

        public GeneralSlashCommands(BotContext db)
        {
            this.db = db;
        }

        [SlashCommand("beer", "Give a beer to Fitz")]
        public async Task GiveBeer(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Thanks for the beer!"));
        }
    }
}