using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Fitz.Core.Commands.Attributes;
using Fitz.Core.Contexts;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Lottery;
using Fitz.Features.Lottery.Models;
using Fitz.Features.Polls;
using Fitz.Features.Polls.Models;
using Fitz.Features.Rename;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Profiles
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class ProfileAdapter : ApplicationCommandModule
    {
        private readonly AccountService _accountService;
        
        public ProfileAdapter(AccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }
        
        [SlashCommand("profile", "You're hired.")]
        [RequireAccount]
        public async Task Profile(InteractionContext ctx)
        {
            Account account = _accountService.FindAccount(ctx.User.Id);
            if (account != null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(CreateAccountEmbed(ctx.User, account)).AsEphemeral(true));
            }
            else
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"Doesn't seem like you have an account. Try running `/signup`.").AsEphemeral(true));
            }
        }
        
        private DiscordEmbed CreateAccountEmbed(DiscordUser user, Account account)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{user.Username}'s Profile")
                .WithDescription($"Account information for {user.Mention}")
                .WithColor(DiscordColor.Blurple)
                .WithThumbnail(user.AvatarUrl)
                .AddField("Beer", account.Beer.ToString(), true)
                .AddField("Created", account.CreatedDate.ToString("yyyy-MM-dd"), true);
                
            return embed.Build();
        }
    }
}