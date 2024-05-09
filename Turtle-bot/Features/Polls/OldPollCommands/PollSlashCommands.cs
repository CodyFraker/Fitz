using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using Fitz.Core.Contexts;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Polls
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class PollSlashCommands : ApplicationCommandModule
    {
        private readonly BotContext db;

        public PollSlashCommands(BotContext db)
        {
            this.db = db;
        }

        [SlashCommand("poll", "I generate polls.")]
        public async Task PingPongAsync(InteractionContext ctx)
        {
            var modal = ModalBuilder.Create("generate_poll")
                .WithTitle("Modal Title")
                .AddComponents(new DiscordTextInputComponent("Poll Title", "Poll Title", "Poll Title", required: true, max_length: 128))
                .AddComponents(new DiscordTextInputComponent("Poll Option 1", "Poll Option 1", "Text asd", required: true, style: DiscordTextInputStyle.Short, max_length: 25))
            .AddComponents(new DiscordTextInputComponent("Poll Option 2", "Poll Option 2", "text asdfsdf.", required: true, style: DiscordTextInputStyle.Short, max_length: 25))
            .AddComponents(new DiscordTextInputComponent("Poll Option 3", "Poll Option 3", "Response answer.", required: false, style: DiscordTextInputStyle.Short, max_length: 25))
            .AddComponents(new DiscordTextInputComponent("Poll Option 4", "Poll Option 4", "Response answer.", required: false, style: DiscordTextInputStyle.Short, max_length: 25));
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
        }

        [SlashCommand("closedPoll", "Generate a poll with Yes/No options.")]
        public async Task ClosedPoll(InteractionContext ctx)
        {
            var modal = ModalBuilder.Create("gen_yesno")
                .WithTitle("Poll Creator")
                .AddComponents(new DiscordTextInputComponent("Poll Title", "Poll Title", "Poll Title", required: true, max_length: 128));
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
        }

        [SlashCommand("thisorthat", "Generate a poll of type this or that.")]
        public async Task ThisOrThatPoll(InteractionContext ctx)
        {
            var modal = ModalBuilder.Create("gen_thisorthat")
                .WithTitle("Poll Creator")
                .AddComponents(new DiscordTextInputComponent("Poll Title", "Poll Title", "Poll Title", required: true, max_length: 128))
            .AddComponents(new DiscordTextInputComponent("This", "This", "This", required: true, style: DiscordTextInputStyle.Short, max_length: 25))
            .AddComponents(new DiscordTextInputComponent("That", "That", "That", required: true, style: DiscordTextInputStyle.Short, max_length: 25));
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
        }
    }
}