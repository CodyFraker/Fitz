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

        [SlashCommand("lookup", "Checkout someone elses profile.")]
        public async Task Lookup(InteractionContext ctx, [Option("User", "Whose profile do you want to see?")] DiscordUser user = null)
        {
            var userId = ctx.User.Id;
            var username = ctx.User.Username;

            _logger.LogInformation("Account lookup started via Discord slash command. UserId: {UserId}, Username: {Username}, LookupUserId: {LookupUserId}", userId, username, user?.Id);

            if (user == null)
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("You need to specify a user.")
                    .AsEphemeral(true));
                return;
            }

            try
            {
                var command = GetAccountCommand.From(user.Id);

                var response = await _getAccountFacade.Execute(command, CancellationToken.None);

                var accountEmbed = AccountEmbed.FromGetAccount(user, response);

                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(accountEmbed)
                    .AsEphemeral(true));

                _logger.LogInformation("Account lookup completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}, LookupUserId: {LookupUserId}", userId, username, user.Id);
            }
            catch (AccountNotFound ex)
            {
                _logger.LogWarning("Account lookup failed - account not found. LookupUserId: {LookupUserId}", ex.UserId);

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Doesn't seem like they have an account.")
                    .AsEphemeral(true));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Account lookup failed - invalid argument. UserId: {UserId}, Username: {Username}, LookupUserId: {LookupUserId}, Error: {Error}", userId, username, user?.Id, ex.Message);

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Invalid request: {ex.Message}")
                    .AsEphemeral(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Account lookup failed - unexpected error. UserId: {UserId}, Username: {Username}, LookupUserId: {LookupUserId}", userId, username, user?.Id);

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("An error occurred while looking up the account. Please try again later.")
                    .AsEphemeral(true));
            }
        }
    }
}
