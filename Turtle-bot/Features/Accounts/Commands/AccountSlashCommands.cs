using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Rename;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    internal class AccountSlashCommands : ApplicationCommandModule
    {
        private readonly BotContext db;
        private readonly AccountService accountService;
        private readonly BankService bankService;

        public AccountSlashCommands(BotContext db, AccountService accountService, BankService bankService)
        {
            this.db = db;
            this.accountService = accountService;
            this.bankService = bankService;
        }

        #region Signup

        [SlashCommand("signup", "Just sign this form.")]
        public async Task signup(InteractionContext ctx)
        {
            if (CheckBasics(ctx))
            {
                try
                {
                    // Create the account
                    await accountService.AddAsync(ctx.User, ctx.Interaction.CreationTimestamp.DateTime);

                    // Give the user some beer
                    await bankService.AwardAccountCreationBonus(ctx.User.Id);

                    Account account = accountService.FindAccount(ctx.User.Id);

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
                            Url = ctx.User.AvatarUrl,
                        },
                        Description = "I collect beer and stupid user data."
                    };

                    DateTime accountCreated = DateTime.Now;
                    accountEmbed.AddField($"**Username**", $"`{account.Username}`", true);
                    accountEmbed.AddField($"**Creation Date**", $"{account.CreatedDate}", true);
                    accountEmbed.AddField($"**Beer**", $"{account.Beer}", true);
                    accountEmbed.AddField($"**Lifetime Beer**", $"{account.LifetimeBeer}", true);
                    accountEmbed.AddField($"**Favorability**", $"{account.Favorability}", true);

                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(accountEmbed.Build()).AsEphemeral(true));

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
                catch (Exception ex)
                {
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"An error occurred: {ex.Message}"));
                    return;
                }
            }
            else
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You may already have an account. Try running `/profile` instead."));
            }
        }

        #endregion Signup

        #region Profile

        [SlashCommand("profile", "You're hired.")]
        [RequireAccount]
        public async Task Profile(InteractionContext ctx)
        {
            Account account = accountService.FindAccount(ctx.User.Id);
            if (account != null)
            {
                DiscordEmbedBuilder accountEmbed = new DiscordEmbedBuilder
                {
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                        Text = $"Account Profile | ID: {account.Id}",
                    },
                    Color = new DiscordColor(52, 114, 53),
                    Timestamp = DateTime.UtcNow,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = ctx.User.AvatarUrl,
                    },
                };
                accountEmbed.AddField($"**Username**", $"`{account.Username}`", true);
                accountEmbed.AddField($"**Beer**", $"{account.Beer}", true);
                accountEmbed.AddField($"**Lifetime Beer**", $"{account.LifetimeBeer}", true);
                accountEmbed.AddField($"**Favorability**", $"{account.Favorability}", true);
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(accountEmbed.Build()).AsEphemeral(true));
            }
            else
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Doesn't seem like you have an account. Try running `/signup`.").AsEphemeral(true));
            }
        }

        #endregion Profile

        private bool CheckBasics(InteractionContext ctx)
        {
            if (accountService.FindAccount(ctx.User.Id) != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}