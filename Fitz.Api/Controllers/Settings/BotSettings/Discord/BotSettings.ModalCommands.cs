using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;
using Fitz.Features.Settings;
using Fitz.Features.Settings.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Api.Controllers.Settings.BotSettings.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class BotSettingsModalCommands(IServiceScopeFactory scopeFactory, ILogger<BotSettingsModalCommands> logger) : ModalCommandModule
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<BotSettingsModalCommands> _logger = logger;

    [ModalCommand("LotteryDuration")]
    public async Task SetLotteryDuration(ModalContext ctx, string lotteryDuration)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Set lottery duration modal submitted. UserId: {UserId}, Username: {Username}, Duration: {Duration}", userId, username, lotteryDuration);

        if (!int.TryParse(lotteryDuration, out int duration) || duration <= 0 || duration > 365)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("Lottery Duration must be between 1 and 365 days.")
                .AsEphemeral(true));
            return;
        }

        var command = new SetLotteryDurationCommand(_scopeFactory);
        var settingsResult = await command.ExecuteAsync(duration);
        
        if (settingsResult.Success)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent("Lottery Duration has been updated.")
                    .AsEphemeral(true));
        }
        else
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent("An error occurred while updating the Lottery Duration.")
                    .AsEphemeral(true));
        }
    }

    [ModalCommand("MaxTickets")]
    public async Task SetMaxTickets(ModalContext ctx, string maxTickets)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Set max tickets modal submitted. UserId: {UserId}, Username: {Username}, MaxTickets: {MaxTickets}", userId, username, maxTickets);

        if (!int.TryParse(maxTickets, out int tickets) || tickets <= 0 || tickets > 999)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("You can only set the max amount of lottery tickets per user between 1 and 999.")
                .AsEphemeral(true));
            return;
        }

        var command = new SetMaxTicketsCommand(_scopeFactory);
        var settingsResult = await command.ExecuteAsync(tickets);
        
        if (settingsResult.Success)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent($"Max tickets for each user was set to {tickets}")
                    .AsEphemeral(true));
        }
        else
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent("An error occurred while updating the Max Tickets.")
                    .AsEphemeral(true));
        }
    }
}
