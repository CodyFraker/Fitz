using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Lottery.Attributes;
using Fitz.Features.Lottery.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    [RequireLotteryAdmin]
    public class LotteryAdminCommands(BotContext db, LotteryService lotteryService, AccountService accountService) : BaseCommandModule
    {
        private readonly BotContext db = db;
        private readonly LotteryService lotteryService = lotteryService;
        private readonly AccountService accountService = accountService;

        [Command("createlottery")]
        [Description("Creates a lottery.")]
        public Task CreateNewLottery(CommandContext ctx)
        {
            return ctx.RespondAsync("beer.");
        }

        [Command("endlottery")]
        [Description("Ends a lottery.")]
        public async Task StopCurrentLottery(CommandContext ctx)
        {
            Models.Lottery drawing = lotteryService.GetCurrentLottery();
            if (drawing == null)
            {
                await ctx.RespondAsync("There is no active lottery.");
                return;
            }

            await lotteryService.UpdateCurrentLottery(endDate: DateTime.UtcNow);
            await ctx.RespondAsync("Set current lottery to end next job cycle.");
        }

        [Command("setprizepool")]
        [Description("Sets the prize pool for the current lottery.")]
        public async Task SetPrizePool(CommandContext ctx, int pool)
        {
            if (pool < 0)
            {
                await ctx.RespondAsync("Prize pool must be greater than 0.");
            }
            Models.Lottery drawing = lotteryService.GetCurrentLottery();
            if (drawing == null)
            {
                await ctx.RespondAsync("There is no active lottery.");
                return;
            }

            var setPoolResult = await this.lotteryService.SetLotteryPrizePoolAsync(pool);

            await ctx.RespondAsync($"{setPoolResult.Message}");
        }

        [Command("fitztickets")]
        [Description("Buys Fitz lottery tickets.")]
        public async Task BuyFitzLotteryTickets(CommandContext ctx, int tickets)
        {
            Models.Lottery drawing = lotteryService.GetCurrentLottery();
            if (drawing == null)
            {
                await ctx.RespondAsync("There is no active lottery.");
                return;
            }

            var buyTicketsResult = await this.lotteryService.BuyTicketsForFitz(tickets);

            await ctx.RespondAsync($"{buyTicketsResult.Message}");
        }

        [Command("mockwin")]
        [Description("Sends you a DM that looks like you won the current lottery.")]
        public async Task MockWinnerLottery(CommandContext ctx)
        {
            Models.Lottery lottery = new Models.Lottery()
            {
                Id = 138,
                Pool = 3600,
                WinningTicket = 1,
            };

            List<Account> winners =
            [
                new Account()
                {
                    Id = ctx.User.Id,
                    Beer = 1000,
                },
            ];

            if (ctx.Channel.IsPrivate)
            {
                await ctx.RespondAsync(embed: this.lotteryService.WinnerEmbed(ctx.Client, lottery, winners));
            }
            else
            {
                DiscordGuild guild = ctx.Guild;
                DiscordMember member = await guild.GetMemberAsync(ctx.User.Id);
                if (member == null || member.IsBot)
                {
                    return;
                }
                // DM The winner to let them know.
                DiscordDmChannel userDMChannel = await member.CreateDmChannelAsync();

                await userDMChannel.SendMessageAsync(embed: this.lotteryService.WinnerEmbed(ctx.Client, lottery, winners));
            }
        }

        [Command("mocksubscription")]
        [Description("Sends you a DM that looks like you were subscribed to the lottery.")]
        public async Task MockLotterySubscription(CommandContext ctx)
        {
            Account account = this.accountService.FindAccount(ctx.User.Id);
            List<Ticket> userTickets = lotteryService.GetUserTickets(account).Data as List<Ticket>;
            Models.Lottery drawing = this.lotteryService.GetCurrentLottery();
            DiscordEmbedBuilder lotteryEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Ticket).Url,
                    Text = $"Lottery #{drawing.Id}",
                },
                Color = new DiscordColor(52, 114, 53),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = DiscordEmoji.FromGuildEmote(ctx.Client, LotteryEmojis.Lottery).Url,
                },
                Title = $"Lottery Subscription",
                Timestamp = DateTime.UtcNow,
                Description = $"Since you've enrolled in the lottery, I went ahead and purchased {userTickets.Count} ticket(s) for you.\n\n" +
                $"You can disable your lottery subscription via `/settings`.\n\n" +
                $"Your current beer balance is: {account.Beer}.\n\n" +
                $"I will not purchase any tickets if your beer is below {account.safeBalance}.\n\n" +
                $"You can change your safe balance at any time via `/settings`",
            };
            if (ctx.Channel.IsPrivate)
            {
                await ctx.RespondAsync(embed: lotteryEmbed.Build());
            }
            else
            {
                DiscordGuild guild = ctx.Guild;
                DiscordMember member = await guild.GetMemberAsync(ctx.User.Id);
                DiscordDmChannel userDMChannel = await member.CreateDmChannelAsync();
                await userDMChannel.SendMessageAsync(embed: lotteryEmbed.Build());
            }
        }

        [Command("mockticket")]
        [Description("Build out buying ticket concept")]
        public async Task BuyTicketConcept(CommandContext ctx, int tickets)
        {
            List<int> conceptTickets = new List<int>();
            for (int t = 0; t < tickets; t++)
            {
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    byte[] data = new byte[4];
                    int ticketNumber = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        rng.GetBytes(data);
                        ticketNumber = BitConverter.ToInt32(data, 0);
                        ticketNumber = Math.Abs(ticketNumber);
                        ticketNumber %= 1000;
                    }
                    conceptTickets.Add(ticketNumber);
                }
            }

            string ticketList = string.Empty;
            foreach (int ticket in conceptTickets)
            {
                ticketList += $"{ticket}, ";
            }
            await ctx.RespondAsync($"{ticketList}");
        }
    }
}