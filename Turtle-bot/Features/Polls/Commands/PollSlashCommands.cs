using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using Fitz.Core.Contexts;
using Fitz.Features.Polls.Models;
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

        [SlashCommand("poll", "Generate a poll.")]
        public async Task GeneratePoll(InteractionContext ctx,
            [Option("Type", "pollType")] PollType pollType = PollType.Number)
        {
            switch (pollType)
            {
                case PollType.Number:
                    var numberModal = ModalBuilder.Create("gen_number")
                        .WithTitle("Create Number Poll")
                        .AddComponents(new DiscordTextInputComponent("Poll Title", "Poll Title", "Poll Title", required: true, max_length: 128))
                        .AddComponents(new DiscordTextInputComponent("Poll Option", "pollOptions", "Comma seperated. Provide up to 10 options.\nExample: Option1,Option2,...Option10.", required: true, style: DiscordTextInputStyle.Paragraph, max_length: 512));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, numberModal);
                    break;

                case PollType.YesOrNo:
                    var yesOrNoModal = ModalBuilder.Create("gen_yesno")
                        .WithTitle("Create Yes/No Poll")
                        .AddComponents(new DiscordTextInputComponent("Poll Title", "Poll Title", "Poll Title", required: true, max_length: 128));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, yesOrNoModal);
                    break;

                case PollType.Color:
                    var colorModal = ModalBuilder.Create("generate_color_poll")
                        .WithTitle("Create Color Poll")
                        .AddComponents(new DiscordTextInputComponent("Poll Title", "Poll Title", "Poll Title", required: true, max_length: 128))
                        .AddComponents(new DiscordTextInputComponent("Poll Option", "pollOptions", "Comma seperated. Provide up to 9 options.\nExample: Option1,Option2...Option9", required: true, style: DiscordTextInputStyle.Paragraph, max_length: 512));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, colorModal);
                    break;

                case PollType.ThisOrThat:
                    var thisOrThatModal = ModalBuilder.Create("gen_thisorthat")
                        .WithTitle("Create This or That Poll")
                        .AddComponents(new DiscordTextInputComponent("Poll Title", "Poll Title", "Poll Title", required: true, max_length: 128))
                        .AddComponents(new DiscordTextInputComponent("This", "This", "This", required: true, style: DiscordTextInputStyle.Short, max_length: 25))
                        .AddComponents(new DiscordTextInputComponent("That", "That", "That", required: true, style: DiscordTextInputStyle.Short, max_length: 25));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, thisOrThatModal);
                    break;

                case PollType.HotTake:
                    var hotTakeModal = ModalBuilder.Create("gen_hottake")
                        .WithTitle("Create Hot Take")
                        .AddComponents(new DiscordTextInputComponent("Poll Title", "Poll Title", "Poll Title", required: true, max_length: 128));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, hotTakeModal);
                    break;

                default:
                    break;
            }
        }
    }
}