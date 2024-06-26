﻿using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class AccountAdminSlashCommands(AccountService accountService, BankService bankService) : ApplicationCommandModule
    {
        private readonly AccountService accountService = accountService;
        private readonly BankService bankService = bankService;

        [SlashCommand("manacc", "Manage account for a specified user.")]
        public async Task ManageAccount(InteractionContext ctx,
            [Option("Action", "test1")] AccountAction accAction = AccountAction.Add,
            [Option("User", "Manage whose account?")] DiscordUser user = null)
        {
            // Check to see if a user was provided
            if (user == null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to specify a user.").AsEphemeral(true));
                return;
            }

            // Check to see if user account exists.
            if (accountService.FindAccount(user.Id) == null)
            {
                if (accAction == AccountAction.Add)
                {
                    await accountService.CreateAccountAsync(user);

                    Account account = accountService.FindAccount(user.Id);
                    DiscordEmbedBuilder accountEmbed = new DiscordEmbedBuilder
                    {
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, ManageRoleEmojis.Warning).Url,
                            Text = $"Account Creation | ID: {account.Id}",
                        },
                        Color = new DiscordColor(52, 114, 53),
                        Timestamp = DateTime.UtcNow,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                        {
                            Url = user.AvatarUrl,
                        },
                        Description = "I collect beer and stupid user data."
                    };
                    accountEmbed.AddField($"**Beer**", $"{account.Beer}", true);
                    accountEmbed.AddField($"**Lifetime Beer**", $"{account.LifetimeBeer}", true);
                    accountEmbed.AddField($"**Favorability**", $"{account.Favorability}", true);

                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(accountEmbed.Build()).WithContent($"Created an account for {user.Username}").AsEphemeral(true));
                    // Give the user the account role
                    if (ctx.Guild.Id == Guilds.Waterbear)
                    {
                        // assign a new role to a user
                        await ctx.Guild.GetMemberAsync(user.Id).Result.GrantRoleAsync(ctx.Guild.GetRole(Roles.Accounts));
                        return;
                    }
                    else
                    {
                        DiscordGuild guild = await ctx.Client.GetGuildAsync(Guilds.Waterbear);
                        DiscordMember discordMember = await guild.GetMemberAsync(ctx.User.Id);

                        // check to see if the user is in the guild
                        if (discordMember == null)
                        {
                            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"That user is not in the guild."));
                            return;
                        }
                        else
                        {
                            await discordMember.GrantRoleAsync(guild.GetRole(Roles.Accounts));
                            return;
                        }
                    }
                }
                if (accAction == AccountAction.Remove)
                {
                    if (accountService.FindAccount(user.Id) == null)
                    {
                        await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{user.Username} does not have an account. No action was taken.").AsEphemeral(true));
                        return;
                    }
                    else
                    {
                        await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Removed account for {user.Username}").AsEphemeral(true));
                    }
                }
            }
            else
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{user.Username} already has an account. No action was taken.").AsEphemeral(true));
            }
        }
    }
}