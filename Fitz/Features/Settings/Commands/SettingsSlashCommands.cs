using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using Fitz.Core.Models;
using Fitz.Features.Settings.Commands;
using Fitz.Features.Settings.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Settings.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class SettingsSlashCommands(IServiceScopeFactory scopeFactory) : ApplicationCommandModule
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        [SlashCommand("botsettings", "Bot settings")]
        public async Task SettingsCommand(InteractionContext ctx,
            [Option("Setting", "Which setting do you wish to modify?")] SettingsAction settingsAction = SettingsAction.AccountCreationBonusAmount)
        {
            var getSettingsQuery = new GetSettingsQuery(scopeFactory);
            var settings = getSettingsQuery.Execute();

            switch (settingsAction)
            {
                case SettingsAction.LotteryDuration:
                    var lotteryDurationModal = ModalBuilder.Create("LotteryDuration")
                        .WithTitle("Set Lottery Duration")
                        .AddComponents(new DiscordTextInputComponent("Duration", "Lottery Duration", "Lottery Duration", required: true, max_length: 11));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, lotteryDurationModal);
                    break;

                case SettingsAction.MaxTickets:
                    var maxTicketsModal = ModalBuilder.Create("MaxTickets")
                        .WithTitle("Set Max Tickets")
                        .AddComponents(new DiscordTextInputComponent("MaxTickets", "Max Tickets", "Max Tickets", required: true, max_length: 11));
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.Modal, maxTicketsModal);
                    break;
            }
        }

        private DiscordEmbed SettingsEmbed(Core.Models.Settings settings)
        {
            DiscordEmbedBuilder settingsEmbed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                },
                Description = "I collect beer and stupid user data.\n" +
                $"Edit your account settings using `/settings`\n"
            };

            return settingsEmbed.Build();
        }
    }
}
