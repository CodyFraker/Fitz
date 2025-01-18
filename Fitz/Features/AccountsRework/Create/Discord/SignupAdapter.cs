using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.AccountsRework.Create.Domain
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class AccountSlashCommands : ApplicationCommandModule
    {
        [SlashCommand("signup", "Just sign this form.")]
        public async Task Signup(InteractionContext ctx)
        {
            Result accountCreationResult = await accountService.CreateAccountAsync(ctx.User);
            if (accountCreationResult.Success)
            {
                Account account = accountCreationResult.Data as Account;
                await this.bankService.AwardAccountCreationBonusAsync(account);

                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .AddEmbed(accountEmbed(ctx.User, account))
                    .AsEphemeral(true));

                // check to see if the user is in the Waterbear guild
                if (ctx.Guild.Id == Guilds.Waterbear)
                {
                    // assign a new role to a user
                    await ctx.Guild.GetMemberAsync(ctx.User.Id).Result.GrantRoleAsync(ctx.Guild.GetRole(Roles.Accounts));
                    return;
                }
                else
                {
                    DiscordGuild guild = await ctx.Client.GetGuildAsync(Guilds.Waterbear);
                    DiscordMember discordMember = await guild.GetMemberAsync(ctx.User.Id);

                    // check to see if the user is in the guild
                    if (discordMember == null)
                    {
                        await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You need to be in the Waterbear guild to get the Accounts role."));
                        return;
                    }
                    else
                    {
                        await discordMember.GrantRoleAsync(guild.GetRole(Roles.Accounts));
                        return;
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You already have an account.").AsEphemeral(true));
            }
        }
    }
}