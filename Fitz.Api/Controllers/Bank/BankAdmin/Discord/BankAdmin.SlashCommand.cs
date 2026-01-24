using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Bank.AwardBonus.Domain;
using Fitz.Api.Controllers.Bank.AwardBonus.Http;
using Fitz.Api.Controllers.Bank.DeductBeer.Domain;
using Fitz.Api.Controllers.Bank.DeductBeer.Http;
using Fitz.Features.Bank.Commands.Attributes;
using Fitz.Features.Bank.Models;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.BankAdmin.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class BankAdminSlashCommand(
    AwardBonusFacade awardBonusFacade,
    DeductBeerFacade deductBeerFacade,
    ILogger<BankAdminSlashCommand> logger) : ApplicationCommandModule
{
    private readonly AwardBonusFacade _awardBonusFacade = awardBonusFacade;
    private readonly DeductBeerFacade _deductBeerFacade = deductBeerFacade;
    private readonly ILogger<BankAdminSlashCommand> _logger = logger;

    [SlashCommand("bank", "Add or remove money/beer from a user.")]
    [RequireBankTeller]
    public async Task BankAdmin(InteractionContext ctx,
        [Option("Action", "Add or remove beer")] BankAction bankAction = BankAction.Add,
        [Option("Amount", "Amount to add/remove")] long amount = 0,
        [Option("User", "User to manage")] DiscordUser discordUser = null)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Bank admin command started via Discord slash command. UserId: {UserId}, Username: {Username}, Action: {Action}, Amount: {Amount}, TargetUser: {TargetUser}", 
            userId, username, bankAction, amount, discordUser?.Id);

        if (discordUser == null)
        {
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("No user was provided.")
                .AsEphemeral(true));
            return;
        }

        if (amount <= 0)
        {
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("Amount must be greater than 0.")
                .AsEphemeral(true));
            return;
        }

        try
        {
            if (bankAction == BankAction.Add)
            {
                var awardBonusCommand = AwardBonusCommand.From(new AwardBonusRequestDto
                {
                    UserId = discordUser.Id,
                    Amount = (int)amount
                });

                var awardBonusResponse = await _awardBonusFacade.Execute(awardBonusCommand, CancellationToken.None);

                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Added {amount} beer to {discordUser.Username}")
                    .AsEphemeral(true));

                _logger.LogInformation("Bank admin command completed successfully - added beer. UserId: {UserId}, TargetUser: {TargetUser}, Amount: {Amount}", 
                    userId, discordUser.Id, amount);
            }
            else if (bankAction == BankAction.Remove)
            {
                var deductBeerCommand = DeductBeerCommand.From(new DeductBeerRequestDto
                {
                    UserId = discordUser.Id,
                    Amount = (int)amount,
                    Reason = Reason.Donated.ToString()
                });

                var deductBeerResponse = await _deductBeerFacade.Execute(deductBeerCommand, CancellationToken.None);

                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Removed {amount} beer from {discordUser.Username}")
                    .AsEphemeral(true));

                _logger.LogInformation("Bank admin command completed successfully - removed beer. UserId: {UserId}, TargetUser: {TargetUser}, Amount: {Amount}", 
                    userId, discordUser.Id, amount);
            }
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Bank admin command failed - account not found. TargetUserId: {TargetUserId}", ex.UserId);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("No user account was found for that user. Try signing them up instead.")
                .AsEphemeral(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bank admin command failed - unexpected error. UserId: {UserId}, Username: {Username}, Action: {Action}, Amount: {Amount}", 
                userId, username, bankAction, amount);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while processing the bank admin command. Please try again later.")
                .AsEphemeral(true));
        }
    }
}
