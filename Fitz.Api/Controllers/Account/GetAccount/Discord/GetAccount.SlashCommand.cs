using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.GetAccount.Domain;
using Fitz.Api.Controllers.Account.Embeds;
using Fitz.Api.Controllers.Account.Exceptions;

namespace Fitz.Api.Controllers.Account.GetAccount.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class GetAccountSlashCommand(GetAccountFacade getAccountFacade, ILogger<GetAccountSlashCommand> logger) : ApplicationCommandModule
    {
        private readonly GetAccountFacade _getAccountFacade = getAccountFacade;
        private readonly ILogger<GetAccountSlashCommand> _logger = logger;

        [SlashCommand("profile", "You're hired.")]
        public async Task Profile(InteractionContext ctx)
        {
            var userId = ctx.User.Id;
            var username = ctx.User.Username;

            _logger.LogInformation("Account retrieval started via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);

            try
            {
                var command = GetAccountCommand.From(userId);

                var response = await _getAccountFacade.Execute(command, CancellationToken.None);

                var accountEmbed = AccountEmbed.FromGetAccount(ctx.User, response);

                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(accountEmbed)
                    .AsEphemeral(true));

                _logger.LogInformation("Account retrieved successfully via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);
            }
            catch (AccountNotFound ex)
            {
                _logger.LogWarning("Account retrieval failed - account not found. UserId: {UserId}", ex.UserId);

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Doesn't seem like you have an account. Try running `/signup`.")
                    .AsEphemeral(true));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Account retrieval failed - invalid argument. UserId: {UserId}, Username: {Username}, Error: {Error}", userId, username, ex.Message);

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Invalid request: {ex.Message}")
                    .AsEphemeral(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Account retrieval failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("An error occurred while retrieving your account. Please try again later.")
                    .AsEphemeral(true));
            }
        }
    }
}
