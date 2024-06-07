using DSharpPlus.SlashCommands;
using Fitz.Core.Models;
using Fitz.Core.Services.Settings;
using Fitz.Features.Accounts;
using System.Threading.Tasks;

namespace Fitz.Core.Commands.Help
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class HelpSlashCommands(SettingsService settingsService, AccountService accountService) : ApplicationCommandModule
    {
        private readonly AccountService accountService = accountService;
        private readonly SettingsService settingsService = settingsService;

        [SlashCommand("help", "Get help with commands")]
        public async Task Help(InteractionContext ctx,
            [Option("with", "What do you need help with?")] HelpAction helpAction = HelpAction.Account)
        {
            Models.Settings settings = settingsService.GetSettings();

            switch (helpAction)
            {
                case HelpAction.Account:
                    await ctx.CreateResponseAsync(DSharpPlus.Entities.DiscordInteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AddEmbed(accountService.AccountHelpEmbed(ctx.Client)).AsEphemeral(true));
                    break;

                case HelpAction.HappyHour:
                    break;

                case HelpAction.Lottery:
                    break;

                case HelpAction.Renames:
                    break;
            }
        }
    }
}