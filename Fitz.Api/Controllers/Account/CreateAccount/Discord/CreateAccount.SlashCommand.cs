using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.CreateAccount.Domain;
using Fitz.Api.Controllers.Account.Embeds;
using Fitz.Api.Controllers.Account.Exceptions;

namespace Fitz.Api.Controllers.Account.CreateAccount.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class CreateAccountSlashCommand(CreateAccountFacade createAccountFacade, ILogger<CreateAccountSlashCommand> logger) : ApplicationCommandModule
    {
        private readonly CreateAccountFacade _createAccountFacade = createAccountFacade;
        private readonly ILogger<CreateAccountSlashCommand> _logger = logger;

        [SlashCommand("signup", "Just sign this form.")]
        public async Task Signup(InteractionContext ctx)
        {
            var userId = ctx.User.Id;
            var username = ctx.User.Username;
            
            _logger.LogInformation("Account creation started via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);
            
            try
            {
                var command = CreateAccountCommand.FromInteractionContext(ctx);

                var response = await _createAccountFacade.Execute(command, CancellationToken.None);

                var accountEmbed = AccountEmbed.FromCreateAccount(ctx.User, response);

                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(accountEmbed)
                    .AsEphemeral(true));
                
                _logger.LogInformation("Account created successfully via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);
            }
            catch (AccountAlreadyExists ex)
            {
                _logger.LogWarning("Account creation failed - account already exists. UserId: {UserId}, Username: {Username}, DiscordId: {DiscordId}", userId, username, ex.DiscordId);
                
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"You already have an account! Discord ID: {ex.DiscordId}")
                    .AsEphemeral(true));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Account creation failed - invalid argument. UserId: {UserId}, Username: {Username}, Error: {Error}", userId, username, ex.Message);
                
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Invalid request: {ex.Message}")
                    .AsEphemeral(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Account creation failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);
                
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("An error occurred while creating your account. Please try again later.")
                    .AsEphemeral(true));
            }
        }
    }
}
