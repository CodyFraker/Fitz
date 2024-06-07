using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using Fitz.Core.Models;
using Fitz.Core.Services.Settings;
using Fitz.Features.Accounts;
using Fitz.Features.Polls.Models;
using Fitz.Variables;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Polls.Polls
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class PollSlashCommands(AccountService accountService, SettingsService settingsService, PollService pollService) : ApplicationCommandModule
    {
        private readonly AccountService accountService = accountService;
        private readonly PollService pollService = pollService;
        private readonly Settings settings = settingsService.GetSettings();

        [SlashCommand("poll", "Generate a poll.")]
        public async Task GeneratePoll(InteractionContext ctx,
            [Option("Type", "pollType")] PollType pollType = PollType.Number)
        {
            var account = accountService.FindAccount(ctx.User.Id);
            if (account == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                       .WithContent("You must have an account to create a poll. Use `/signup` to create an account.").AsEphemeral(true));
                return;
            }

            if (account.Beer < (settings.PollSubmittedPenalty + settings.PollDeclinedPenalty))
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                                          .WithContent($"You need at least {settings.PollSubmittedPenalty + settings.PollDeclinedPenalty} beer to create a poll.").AsEphemeral(true));
                return;
            }

            if (this.pollService.GetPollsSubmittedByUser(account.Id).Where(x => x.Status == PollStatus.Pending).Count() >= settings.MaxPendingPolls)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"You have reached the maximum number of polls you can submit. You can have a maximum of {settings.MaxPendingPolls} polls submitted at a time.").AsEphemeral(true));
                return;
            }

            switch (pollType)
            {
                case PollType.Number:
                    var numberModal = ModalBuilder.Create("gen_number")
                        .WithTitle("Create Number Poll")
                        .AddComponents(new DiscordTextInputComponent($"{DiscordEmoji.FromName(ctx.Client, ":bar_chart:")}Poll Title", "genNumberPollTitle", "Number Poll Title", required: true, max_length: 128))
                        .AddComponents(new DiscordTextInputComponent(
                            $"Poll Options{DiscordEmoji.FromName(ctx.Client, ":one:")}{DiscordEmoji.FromName(ctx.Client, ":two:")}{DiscordEmoji.FromName(ctx.Client, ":three:")}{DiscordEmoji.FromName(ctx.Client, ":four:")}{DiscordEmoji.FromName(ctx.Client, ":five:")}{DiscordEmoji.FromName(ctx.Client, ":six:")}{DiscordEmoji.FromName(ctx.Client, ":seven:")}{DiscordEmoji.FromName(ctx.Client, ":eight:")}{DiscordEmoji.FromName(ctx.Client, ":nine:")}{DiscordEmoji.FromName(ctx.Client, ":keycap_ten:")}",
                            "pollOptions",
                            "Comma seperated. Provide up to 10 options.\nExample:Option1,Option2,Option3,Option4...Option10",
                            required: true,
                            style: DiscordTextInputStyle.Paragraph, max_length: 512));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, numberModal);
                    break;

                case PollType.YesOrNo:
                    var yesOrNoModal = ModalBuilder.Create("gen_yesno")
                        .WithTitle("Create Yes/No Poll")
                        .AddComponents(new DiscordTextInputComponent($"{DiscordEmoji.FromName(ctx.Client, ":bar_chart:")}Poll Title", "yesnoPollTitle", "Yes or No Poll Title", required: true, max_length: 128));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, yesOrNoModal);
                    break;

                case PollType.Color:
                    var colorModal = ModalBuilder.Create("generate_color_poll")
                        .WithTitle("Create Color Poll")
                        .AddComponents(new DiscordTextInputComponent($"{DiscordEmoji.FromName(ctx.Client, ":bar_chart:")}Poll Title", "colorPollTitle", "Color Poll Title", required: true, max_length: 128))
                        .AddComponents(new DiscordTextInputComponent(
                            $"Poll Options{DiscordEmoji.FromName(ctx.Client, ":blue_circle:")}{DiscordEmoji.FromName(ctx.Client, ":green_circle:")}{DiscordEmoji.FromName(ctx.Client, ":orange_circle:")}{DiscordEmoji.FromName(ctx.Client, ":purple_circle:")}{DiscordEmoji.FromName(ctx.Client, ":red_circle:")}{DiscordEmoji.FromName(ctx.Client, ":yellow_circle:")}{DiscordEmoji.FromName(ctx.Client, ":brown_circle:")}{DiscordEmoji.FromName(ctx.Client, ":black_circle:")}{DiscordEmoji.FromName(ctx.Client, ":white_circle:")}",
                            "pollOptions", $"Comma seperated. Provide up to 9 options.\nExample:\nOption1,Option2,Option3,Option4,Option5...Option9",
                            required: true,
                            style: DiscordTextInputStyle.Paragraph, max_length: 512));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, colorModal);
                    break;

                case PollType.ThisOrThat:
                    var thisOrThatModal = ModalBuilder.Create("gen_thisorthat")
                        .WithTitle("Create This or That Poll")
                        .AddComponents(new DiscordTextInputComponent($"{DiscordEmoji.FromName(ctx.Client, ":bar_chart:")}Poll Title", "thisOrThatPollTitle", "", required: true, max_length: 128))
                        .AddComponents(new DiscordTextInputComponent($"This{DiscordEmoji.FromName(ctx.Client, ":point_left:")}", "This", "This", required: true, style: DiscordTextInputStyle.Short, max_length: 25))
                        .AddComponents(new DiscordTextInputComponent($"{DiscordEmoji.FromName(ctx.Client, ":point_right:")}That", "That", "That", required: true, style: DiscordTextInputStyle.Short, max_length: 25));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, thisOrThatModal);
                    break;

                case PollType.HotTake:
                    var hotTakeModal = ModalBuilder.Create("gen_hottake")
                        .WithTitle("Create Hot Take")
                        .AddComponents(new DiscordTextInputComponent($"{DiscordEmoji.FromName(ctx.Client, ":bar_chart:")}Poll Title", "Poll Title", "Poll Title", required: true, max_length: 128));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, hotTakeModal);
                    break;

                default:
                    break;
            }
        }
    }
}