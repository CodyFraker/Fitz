using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Core.Models;
using Fitz.Core.Services.Settings;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Lottery.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Jobs.Services
{
    public class LotterySubscriptionHandler
    {
        private readonly DiscordClient dClient;
        private readonly ILotteryService lotteryService;
        private readonly BankService bankService;
        private readonly AccountService accountService;
        private readonly SettingsService settingsService;

        public LotterySubscriptionHandler(
            DiscordClient dClient,
            ILotteryService lotteryService,
            BankService bankService,
            AccountService accountService,
            SettingsService settingsService)
        {
            this.dClient = dClient;
            this.lotteryService = lotteryService;
            this.bankService = bankService;
            this.accountService = accountService;
            this.settingsService = settingsService;
        }

        public async Task HandleLotterySubscriptions()
        {
            Settings settings = settingsService.GetSettings();
            List<Account> lotterySubscribers = accountService.GetLotterySubscribers();

            foreach (Account subscriber in lotterySubscribers)
            {
                if (subscriber == null)
                {
                    continue;
                }

                if (subscriber.subscribeToLottery && subscriber.SubscribeTickets != 0)
                {
                    await ProcessSubscriber(subscriber, settings);
                }
            }
        }

        private async Task ProcessSubscriber(Account subscriber, Settings settings)
        {
            // If the user's beer is equal to or less than the safe balance, do nothing.
            if (subscriber.Beer <= subscriber.safeBalance)
            {
                return;
            }

            // If the user's beer is greater than the safe balance, buy tickets
            if (subscriber.Beer >= subscriber.SubscribeTickets * settings.TicketCost)
            {
                // Check to see if the user has already bought tickets.
                List<Ticket> userTickets = lotteryService.GetUserTickets(subscriber).Data as List<Ticket>;

                if (userTickets.Count == settings.MaxTickets)
                {
                    // TODO: DM the user that the lottery tried to buy tickets for them but they were at the limit.
                    return;
                }

                if (userTickets.Count + subscriber.SubscribeTickets > settings.MaxTickets)
                {
                    // If the user is trying to buy more tickets than the limit, only buy up to the limit.
                    int ticketsToBuy = settings.MaxTickets - userTickets.Count;
                    if (ticketsToBuy <= 0)
                    {
                        return;
                    }

                    await PurchaseTickets(subscriber, ticketsToBuy);
                }
                else
                {
                    // Buy the tickets for the user.
                    await PurchaseTickets(subscriber, subscriber.SubscribeTickets);
                }
            }
        }

        private async Task PurchaseTickets(Account subscriber, int ticketCount)
        {
            await bankService.PurchaseLotteryTicket(subscriber, ticketCount);
            await lotteryService.CreateTicket(subscriber, ticketCount);
            await lotteryService.AddToPool(ticketCount);

            List<Ticket> userTickets = lotteryService.GetUserTickets(subscriber).Data as List<Ticket>;
            await MessageEnrolleeSuccess(subscriber, userTickets);
        }

        private async Task MessageEnrolleeSuccess(Account account, List<Ticket> userTickets)
        {
            try
            {
                // Get discord user
                DiscordUser user = await dClient.GetUserAsync(account.Id);

                // Get current lottery
                Models.Lottery drawing = lotteryService.GetCurrentLottery();

                DiscordEmbedBuilder lotteryEmbed = new DiscordEmbedBuilder
                {
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Ticket).Url,
                        Text = $"Lottery #{drawing.Id}",
                    },
                    Color = new DiscordColor(52, 114, 53),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
                    },
                    Title = $"Lottery Subscription",
                    Timestamp = DateTime.UtcNow,
                    Description = $"Since you've enrolled in the lottery, I went ahead and purchased {userTickets.Count} ticket(s) for you.\n\n" +
                    $"You can disable your lottery subscription via `/settings`.\n\n" +
                    $"Your current beer balance is: {account.Beer}.\n\n" +
                    $"I will not purchase any tickets if your beer is below {account.safeBalance}.\n\n" +
                    $"You can change your safe balance at any time via `/settings`",
                };

                DiscordGuild guild = await dClient.GetGuildAsync(Guilds.Waterbear);
                DiscordMember member = await guild.GetMemberAsync(user.Id);
                DiscordDmChannel userDMChannel = await member.CreateDmChannelAsync();
                await Task.Delay(5000);
                await userDMChannel.SendMessageAsync(embed: lotteryEmbed.Build());
            }
            catch (Exception)
            {
                // Silently handle exceptions when messaging users
            }
        }
    }
}