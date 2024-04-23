using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts;
using Fitz.Features.Bank;
using Fitz.Variables.Emojis;
using System;
using System.Threading.Tasks;

namespace Fitz.Core.Commands.Account
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

        [SlashCommand("signup", "Just sign this form.")]
        public async Task signup(InteractionContext ctx)
        {
            if (this.CheckBasics(ctx))
            {
                try
                {
                    // Create the account
                    await this.accountService.AddAsync(ctx.User, ctx.Interaction.CreationTimestamp.DateTime);

                    // Give the user some beer
                    await this.bankService.AwardAccountCreationBonus(ctx.User.Id);

                    Features.Accounts.Models.Account account = accountService.FindAccount(ctx.User.Id);

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

        [SlashCommand("profile", "You're hired.")]
        public async Task Profile(InteractionContext ctx)
        {
            if (CheckBasics(ctx) == false)
            {
                Features.Accounts.Models.Account account = accountService.FindAccount(ctx.User.Id);
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
                await ctx.Channel.SendMessageAsync($"Doesn't appear that you have an account.");
                return;
            }
        }

        private bool CheckBasics(InteractionContext ctx)
        {
            if (this.accountService.FindAccount(ctx.User.Id) != null)
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