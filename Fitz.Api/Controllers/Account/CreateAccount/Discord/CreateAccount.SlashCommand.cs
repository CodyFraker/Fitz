using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.CreateAccount.Domain;
using Fitz.Api.Controllers.Account.Embeds;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Core.Discord;
using Fitz.Database.Entities;
using Fitz.Variables;
using System.Security.Principal;
using System.Threading;

namespace Fitz.Api.Controllers.Account.CreateAccount.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class CreateAccountSlashCommand(CreateAccountFacade createAccountFacade) : ApplicationCommandModule
    {
        private readonly CreateAccountFacade _createAccountFacade = createAccountFacade;

        [SlashCommand("signup", "Just sign this form.")]
        public async Task Signup(InteractionContext ctx)
        {
            try
            {
                var command = CreateAccountCommand.FromInteractionContext(ctx);

                var response = await _createAccountFacade.Execute(command, CancellationToken.None);

                var accountEmbed = AccountEmbed.FromCreateAccount(ctx.User, response);

                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(accountEmbed)
                    .AsEphemeral(true));
            }
            catch (AccountAlreadyExists ex)
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"You already have an account! Discord ID: {ex.DiscordId}")
                    .AsEphemeral(true));
            }
            catch (ArgumentException ex)
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Invalid request: {ex.Message}")
                    .AsEphemeral(true));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("An error occurred while creating your account. Please try again later.")
                    .AsEphemeral(true));
            }
        }
    }
}
