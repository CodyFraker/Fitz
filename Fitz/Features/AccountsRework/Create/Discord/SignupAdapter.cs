using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Features.AccountsRework.Create.Domain;
using System.Threading.Tasks;
using Fitz.Features.AccountsRework.Create.Discord.Embeds;
using Fitz.Features.AccountsRework.Create.Discord.Attributes;

/******************************FOR KYLE********************************/
/* This replaces Features/Accounts/Commands/AccountSlashCommands.cs   */
/**********************************************************************/

namespace Fitz.Features.AccountsRework.Create.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class AccountSlashCommands(CreateAccountConductor createAccountConductor) : ApplicationCommandModule
    {
        private readonly CreateAccountConductor createAccountConductor = createAccountConductor;

        [SlashCommand("signup", "Just sign this form.")]
        [CreateAccountACL]
        public async Task Signup(InteractionContext ctx)
        {
            var createAccountDto = new CreateAccountDto
            {
                Context = ctx,
            };

            var CreateAccountCommand = new AccountsMapper().MapCommandFromDto(createAccountDto);
            var CreateAccountResponse = await createAccountConductor.CreateAccount(CreateAccountCommand);

            if (CreateAccountResponse.StatusCode != System.Net.HttpStatusCode.Created)
            {
                switch (CreateAccountResponse.StatusCode)
                {
                    case System.Net.HttpStatusCode.Conflict:
                        await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder()
                            .AddEmbed(new CreateAccountConflictedEmbed().BuildEmbed(ctx.Client, ctx.User))
                            .AsEphemeral(true));
                        break;

                    case System.Net.HttpStatusCode.InternalServerError:
                        await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder()
                            .AddEmbed(new CreateAccountFailureEmbed().BuildEmbed(ctx.Client, ctx.User))
                            .AsEphemeral(true));
                        break;

                    default:
                        await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder()
                            .AddEmbed(new CreateAccountFailureEmbed().BuildEmbed(ctx.Client, ctx.User))
                            .AsEphemeral(true));
                        break;
                }
            }

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(new CreatedAccountSuccessEmbed().BuildEmbed(ctx.Client, ctx.User, CreateAccountResponse.Account))
                .AsEphemeral(true));
        }
    }
}