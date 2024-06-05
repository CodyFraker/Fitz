using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using Fitz.Core.Contexts;
using Fitz.Core.Models;
using Fitz.Core.Services.Settings;
using System;
using System.Threading.Tasks;

namespace Fitz.Core.Commands.Settings
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public sealed class SettingsCommands(SettingsService settingsService, BotContext botContext, DiscordClient discordClient) : ApplicationCommandModule
    {
        private readonly SettingsService settingsService = settingsService;

        [SlashCommand("botsettings", "Bot settings")]
        public async Task SettingsCommand(InteractionContext ctx,
            [Option("Setting", "Which setting do you wish to modify?")] SettingsAction settingsAction = SettingsAction.AccountCreationBonusAmount)
        {
            Models.Settings settings = settingsService.GetSettings();

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

        private DiscordEmbed SettingsEmbed(Models.Settings settings)
        {
            DiscordEmbedBuilder settingsEmbed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    //IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, ManageRoleEmojis.Warning).Url,
                    //Text = $"Account Creation | ID: {account.Id}",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    //Url = user.AvatarUrl,
                },
                Description = "I collect beer and stupid user data.\n" +
                $"Edit your account settings using `/settings`\n"
            };

            return settingsEmbed.Build();
        }
    }
}