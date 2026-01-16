using DSharpPlus.SlashCommands;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Queries;
using Fitz.Features.Settings.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Fitz.Core.Commands.Help
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class HelpSlashCommands(IServiceScopeFactory scopeFactory) : ApplicationCommandModule
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        [SlashCommand("help", "Get help with commands")]
        public async Task Help(InteractionContext ctx,
            [Option("with", "What do you need help with?")] HelpAction helpAction = HelpAction.Account)
        {
            var getSettingsQuery = new GetSettingsQuery(scopeFactory);
            var settings = getSettingsQuery.Execute();

            switch (helpAction)
            {
                case HelpAction.Account:
                    var getAccountHelpEmbedQuery = new GetAccountHelpEmbedQuery();
                    await ctx.CreateResponseAsync(DSharpPlus.Entities.DiscordInteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AddEmbed(getAccountHelpEmbedQuery.Execute(ctx.Client)).AsEphemeral(true));
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