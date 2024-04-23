using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fitz.Core.Commands.Polls
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class PollSlashCommands : ApplicationCommandModule
    {
        private readonly BotContext db;

        public PollSlashCommands(BotContext db, AccountService accountService)
        {
            this.db = db;
        }

        [SlashCommand("poll", "I generate polls.")]
        public async Task PingPongAsync(InteractionContext ctx)
        {
            List<DiscordSelectComponentOption> discordSelectComponentOptions = new List<DiscordSelectComponentOption>
            {
                new DiscordSelectComponentOption("Option 1", "Option 1","Meowmix"),
                new DiscordSelectComponentOption("Option 2", "Option 2","test")
            };

            var modal = ModalBuilder.Create("generate_poll")
                .WithTitle("Modal Title")
                .AddComponents(new DiscordTextInputComponent("Poll Title", "Poll Title", "Poll Title", required: true, max_length: 128))
                .AddComponents(new DiscordSelectComponent("options", "Poll Option 1", [new DiscordSelectComponentOption("Option 1", "Option 1", "Meowmix"), new DiscordSelectComponentOption("Option 2", "Option 2", "test")]));
            //.AddComponents(new DiscordTextInputComponent("Poll Option 2", "Poll Option 2", "text asdfsdf.", required: true, style: DiscordTextInputStyle.Short, max_length: 25))
            //.AddComponents(new DiscordTextInputComponent("Poll Option 3", "Poll Option 3", "Response answer.", required: false, style: DiscordTextInputStyle.Short, max_length: 25))
            //.AddComponents(new DiscordTextInputComponent("Poll Option 4", "Poll Option 4", "Response answer.", required: false, style: DiscordTextInputStyle.Short, max_length: 25))
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
        }
    }
}