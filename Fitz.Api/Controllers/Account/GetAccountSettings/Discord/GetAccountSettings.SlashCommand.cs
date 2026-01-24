using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.Embeds;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Account.GetAccountSettings.Domain;
using Fitz.Api.Controllers.Account.SetLotterySubscribe.Domain;
using Fitz.Api.Controllers.Account.SetLotterySubscribe.Http;
using Fitz.Api.Controllers.Account.SetSafeBalance.Domain;
using Fitz.Api.Controllers.Account.SetSafeBalance.Http;
using Fitz.Api.Controllers.Account.SetTicketAmount.Domain;
using Fitz.Api.Controllers.Account.SetTicketAmount.Http;

namespace Fitz.Api.Controllers.Account.GetAccountSettings.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class GetAccountSettingsSlashCommand(
    GetAccountSettingsFacade getAccountSettingsFacade,
    SetLotterySubscribeFacade setLotterySubscribeFacade,
    SetSafeBalanceFacade setSafeBalanceFacade,
    SetTicketAmountFacade setTicketAmountFacade,
    DiscordClient discordClient,
    ILogger<GetAccountSettingsSlashCommand> logger) : ApplicationCommandModule
{
    private readonly GetAccountSettingsFacade _getAccountSettingsFacade = getAccountSettingsFacade;
    private readonly SetLotterySubscribeFacade _setLotterySubscribeFacade = setLotterySubscribeFacade;
    private readonly SetSafeBalanceFacade _setSafeBalanceFacade = setSafeBalanceFacade;
    private readonly SetTicketAmountFacade _setTicketAmountFacade = setTicketAmountFacade;
    private readonly DiscordClient _discordClient = discordClient;
    private readonly ILogger<GetAccountSettingsSlashCommand> _logger = logger;

    [SlashCommand("settings", "Change your account settings")]
    public async Task AccountSettings(InteractionContext ctx)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Account settings command started via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);

        try
        {
            var command = GetAccountSettingsCommand.From(userId);
            var model = await _getAccountSettingsFacade.Execute(command, CancellationToken.None);

            DiscordButtonComponent subscribeBtn;
            if (model.SubscribeToLottery)
            {
                subscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"subscribe_button_{userId}", "Unsubscribe To Lottery", false);
            }
            else
            {
                subscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"subscribe_button_{userId}", "Subscribe To Lottery", false);
            }

            DiscordButtonComponent setSafeBalance = new DiscordButtonComponent(DiscordButtonStyle.Primary, $"setSafeBalance_{userId}", "Set Safe Balance", false);
            DiscordButtonComponent setTicketAmount = new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"setTicketAmount_{userId}", "Set Ticket Amount", false);

            await ctx.DeferAsync(true);

            var embed = AccountSettingsEmbed.FromGetAccountSettings(_discordClient, ctx.User, model);

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(embed)
                .AddComponents(subscribeBtn, setSafeBalance, setTicketAmount).AsEphemeral(true));

            _discordClient.ComponentInteractionCreated += async (sender, args) =>
            {
                if (args.User.Id != userId) return;

                if (args.Id == $"subscribe_button_{userId}")
                {
                    try
                    {
                        await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

                        var setSubscribeCommand = SetLotterySubscribeCommand.From(new SetLotterySubscribeRequestDto
                        {
                            UserId = userId,
                            Subscribe = !model.SubscribeToLottery
                        });

                        await _setLotterySubscribeFacade.Execute(setSubscribeCommand, CancellationToken.None);

                        var updatedCommand = GetAccountSettingsCommand.From(userId);
                        var updatedModel = await _getAccountSettingsFacade.Execute(updatedCommand, CancellationToken.None);

                        DiscordButtonComponent updatedSubscribeBtn;
                        if (updatedModel.SubscribeToLottery)
                        {
                            updatedSubscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"subscribe_button_{userId}", "Unsubscribe To Lottery", false);
                        }
                        else
                        {
                            updatedSubscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"subscribe_button_{userId}", "Subscribe To Lottery", false);
                        }

                        DiscordButtonComponent updatedSetSafeBalance = new DiscordButtonComponent(DiscordButtonStyle.Primary, $"setSafeBalance_{userId}", "Set Safe Balance", false);
                        DiscordButtonComponent updatedSetTicketAmount = new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"setTicketAmount_{userId}", "Set Ticket Amount", false);

                        var updatedEmbed = AccountSettingsEmbed.FromGetAccountSettings(_discordClient, ctx.User, updatedModel);

                        await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                            .AddEmbed(updatedEmbed)
                            .AddComponents(updatedSubscribeBtn, updatedSetSafeBalance, updatedSetTicketAmount));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update lottery subscription. UserId: {UserId}", userId);
                        await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                            .WithContent("Failed to update lottery subscription. Please try again."));
                    }
                }
                else if (args.Id == $"setSafeBalance_{userId}")
                {
                    var numberModal = ModalBuilder.Create("set_safe_balance")
                        .WithTitle("Set Safe Balance")
                        .AddComponents(new DiscordTextInputComponent("Safe Balance", "safe_balance", "Safe Balance", required: true, max_length: 11));

                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, numberModal);

                    _discordClient.ModalSubmitted += async (dClientModal, modalSubmitEvent) =>
                    {
                        if (modalSubmitEvent.Interaction.User.Id != userId) return;

                        if (modalSubmitEvent.Values.ContainsKey("safe_balance"))
                        {
                            try
                            {
                                int safeBalance = int.Parse(modalSubmitEvent.Values["safe_balance"]);
                                
                                var setSafeBalanceCommand = SetSafeBalanceCommand.From(new SetSafeBalanceRequestDto
                                {
                                    UserId = userId,
                                    SafeBalance = safeBalance
                                });

                                await _setSafeBalanceFacade.Execute(setSafeBalanceCommand, CancellationToken.None);

                                var updatedCommand = GetAccountSettingsCommand.From(userId);
                                var updatedModel = await _getAccountSettingsFacade.Execute(updatedCommand, CancellationToken.None);

                                DiscordButtonComponent updatedSubscribeBtn;
                                if (updatedModel.SubscribeToLottery)
                                {
                                    updatedSubscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"subscribe_button_{userId}", "Unsubscribe To Lottery", false);
                                }
                                else
                                {
                                    updatedSubscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"subscribe_button_{userId}", "Subscribe To Lottery", false);
                                }

                                DiscordButtonComponent updatedSetSafeBalance = new DiscordButtonComponent(DiscordButtonStyle.Primary, $"setSafeBalance_{userId}", "Set Safe Balance", false);
                                DiscordButtonComponent updatedSetTicketAmount = new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"setTicketAmount_{userId}", "Set Ticket Amount", false);

                                var updatedEmbed = AccountSettingsEmbed.FromGetAccountSettings(_discordClient, ctx.User, updatedModel);

                                await modalSubmitEvent.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                                    new DiscordInteractionResponseBuilder()
                                    .WithContent("Updated your safe balance.")
                                    .AsEphemeral(true));

                                await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                                    .AddEmbed(updatedEmbed)
                                    .AddComponents(updatedSubscribeBtn, updatedSetSafeBalance, updatedSetTicketAmount));
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to set safe balance. UserId: {UserId}", userId);
                                await modalSubmitEvent.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                                    new DiscordInteractionResponseBuilder()
                                    .WithContent("Failed to set safe balance. Please try again.")
                                    .AsEphemeral(true));
                            }
                        }
                    };
                }
                else if (args.Id == $"setTicketAmount_{userId}")
                {
                    var ticketModal = ModalBuilder.Create("set_ticket_amount")
                        .WithTitle("Set Ticket Amount")
                        .AddComponents(new DiscordTextInputComponent("Tickets", "safe_tickets", "Tickets", required: true, max_length: 11));

                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, ticketModal);

                    _discordClient.ModalSubmitted += async (dClientModal, modalSubmitEvent) =>
                    {
                        if (modalSubmitEvent.Interaction.User.Id != userId) return;

                        if (modalSubmitEvent.Values.ContainsKey("safe_tickets"))
                        {
                            try
                            {
                                int ticketAmount = int.Parse(modalSubmitEvent.Values["safe_tickets"]);
                                
                                var setTicketAmountCommand = SetTicketAmountCommand.From(new SetTicketAmountRequestDto
                                {
                                    UserId = userId,
                                    Amount = ticketAmount
                                });

                                await _setTicketAmountFacade.Execute(setTicketAmountCommand, CancellationToken.None);

                                var updatedCommand = GetAccountSettingsCommand.From(userId);
                                var updatedModel = await _getAccountSettingsFacade.Execute(updatedCommand, CancellationToken.None);

                                DiscordButtonComponent updatedSubscribeBtn;
                                if (updatedModel.SubscribeToLottery)
                                {
                                    updatedSubscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"subscribe_button_{userId}", "Unsubscribe To Lottery", false);
                                }
                                else
                                {
                                    updatedSubscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"subscribe_button_{userId}", "Subscribe To Lottery", false);
                                }

                                DiscordButtonComponent updatedSetSafeBalance = new DiscordButtonComponent(DiscordButtonStyle.Primary, $"setSafeBalance_{userId}", "Set Safe Balance", false);
                                DiscordButtonComponent updatedSetTicketAmount = new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"setTicketAmount_{userId}", "Set Ticket Amount", false);

                                var updatedEmbed = AccountSettingsEmbed.FromGetAccountSettings(_discordClient, ctx.User, updatedModel);

                                await modalSubmitEvent.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                                    new DiscordInteractionResponseBuilder()
                                    .WithContent("Updated your safe ticket amount.")
                                    .AsEphemeral(true));

                                await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                                    .AddEmbed(updatedEmbed)
                                    .AddComponents(updatedSubscribeBtn, updatedSetSafeBalance, updatedSetTicketAmount));
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to set ticket amount. UserId: {UserId}", userId);
                                await modalSubmitEvent.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                                    new DiscordInteractionResponseBuilder()
                                    .WithContent("Failed to set ticket amount. Please try again.")
                                    .AsEphemeral(true));
                            }
                        }
                    };
                }
            };

            _logger.LogInformation("Account settings command completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Account settings failed - account not found. UserId: {UserId}", ex.UserId);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"Doesn't seem like you have an account. Try running `/signup`.")
                .AsEphemeral(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Account settings failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while retrieving your account settings. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
