using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Variables.Emojis;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class AccountAdminSlashCommands : ApplicationCommandModule
    {
        private readonly BotContext db;
        private readonly AccountService accountService;
        private readonly BankService bankService;

        public AccountAdminSlashCommands(BotContext db, AccountService accountService, BankService bankService)
        {
            this.db = db;
            this.accountService = accountService;
            this.bankService = bankService;
        }

        public enum AccountAction
        {
            [ChoiceName("Create")]
            Add,

            [ChoiceName("Remove")]
            Remove
        }

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
                    await accountService.AddAsync(user, ctx.Interaction.CreationTimestamp.DateTime);
                    await bankService.AwardAccountCreationBonus(user.Id);

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
                    accountEmbed.AddField($"**Username**", $"`{account.Username}`", true);
                    accountEmbed.AddField($"**Beer**", $"{account.Beer}", true);
                    accountEmbed.AddField($"**Lifetime Beer**", $"{account.LifetimeBeer}", true);
                    accountEmbed.AddField($"**Favorability**", $"{account.Favorability}", true);
                    await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(accountEmbed.Build()).AsEphemeral(true));
                }
            }
        }
    }
}