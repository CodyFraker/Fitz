using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Settings.GetSettings.Domain;
using Fitz.Core.Models;

namespace Fitz.Api.Controllers.Settings.BotSettings.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class BotSettingsSlashCommand(
    GetSettingsFacade getSettingsFacade,
    DiscordClient discordClient,
    ILogger<BotSettingsSlashCommand> logger) : ApplicationCommandModule
{
    private readonly GetSettingsFacade _getSettingsFacade = getSettingsFacade;
    private readonly DiscordClient _discordClient = discordClient;
    private readonly ILogger<BotSettingsSlashCommand> _logger = logger;

    [SlashCommand("botsettings", "Bot settings")]
    public async Task BotSettings(InteractionContext ctx,
        [Option("Setting", "Which setting do you wish to modify?")] SettingsAction settingsAction = SettingsAction.AccountCreationBonusAmount)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Bot settings command started via Discord slash command. UserId: {UserId}, Username: {Username}, Setting: {Setting}", userId, username, settingsAction);

        try
        {
            var command = GetSettingsCommand.From();
            var settings = await _getSettingsFacade.Execute(command, CancellationToken.None);

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

                default:
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .WithContent($"Setting {settingsAction} is not yet implemented in the new command structure.")
                        .AsEphemeral(true));
                    break;
            }

            _logger.LogInformation("Bot settings command completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}, Setting: {Setting}", userId, username, settingsAction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bot settings command failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while processing the bot settings command. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
