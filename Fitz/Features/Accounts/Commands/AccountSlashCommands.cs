using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Core.Discord;
using Fitz.Database.Entities;
using Fitz.Features.Accounts.Commands;
using Fitz.Database.Entities;
using Fitz.Features.Accounts.Queries;
using Fitz.Features.Bank;
using Fitz.Features.Lottery;
using Fitz.Database.Entities;
using Fitz.Features.Polls;
using Fitz.Database.Entities;
using Fitz.Features.Rename;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public sealed class AccountSlashCommands : ApplicationCommandModule
    {
        private readonly DiscordClient dClient;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly BotLog botLog;
        private readonly BankService bankService;
        private readonly PollService pollService;
        private readonly LotteryService lotteryService;
        private readonly RenameService renameService;

        public AccountSlashCommands(IServiceScopeFactory scopeFactory, BotLog botLog, BankService bankService, DiscordClient dClient, PollService pollService, LotteryService lotteryService, RenameService renameService)
        {
            this.scopeFactory = scopeFactory;
            this.botLog = botLog;
            this.dClient = dClient;
            this.renameService = renameService;
            this.lotteryService = lotteryService;
            this.pollService = pollService;
            this.bankService = bankService;

            #region Account Creation Interactions

            this.dClient.ComponentInteractionCreated += this.HandleAccountSettingsEvent;

            #endregion Account Creation Interactions
        }

        #region Signup

        [SlashCommand("signup", "Just sign this form.")]
        public async Task Signup(InteractionContext ctx)
        {
            var createAccountCommand = new CreateAccountCommand(scopeFactory, botLog);
            var accountCreationResult = await createAccountCommand.ExecuteAsync(ctx.User);
            if (accountCreationResult.Success)
            {
                AccountEntity account = accountCreationResult.Data as AccountEntity;
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

        #endregion Signup

        #region Profile

        [SlashCommand("profile", "You're hired.")]
        [RequireAccount]
        public async Task Profile(InteractionContext ctx)
        {
            var findAccountQuery = new FindAccountQuery(scopeFactory);
            AccountEntity account = findAccountQuery.Execute(ctx.User.Id);
            if (account != null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(accountEmbed(ctx.User, account)).AsEphemeral(true));
            }
            else
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"Doesn't seem like you have an account. Try running `/signup`.").AsEphemeral(true));
            }
        }

        #endregion Profile

        #region Profile Lookup

        [SlashCommand("lookup", "Checkout someone elses profile.")]
        [RequireAccount]
        public async Task ProfileLookup(InteractionContext ctx, [Option("User", "Whose profile do you want to see?")] DiscordUser user = null)
        {
            var findAccountQuery = new FindAccountQuery(scopeFactory);
            AccountEntity account = findAccountQuery.Execute(user.Id);
            if (account != null)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(accountEmbed(user, account)).AsEphemeral(true));
            }
            else
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"Doesn't seem like they have an account.").AsEphemeral(true));
            }
        }

        #endregion Profile Lookup

        #region Settings

        [SlashCommand("settings", "Change your account settings")]
        [RequireAccount]
        public async Task AccountSettings(InteractionContext ctx)
        {
            var findAccountQuery = new FindAccountQuery(scopeFactory);
            AccountEntity account = findAccountQuery.Execute(ctx.User.Id);
            DiscordButtonComponent subscribeBtn;
            if (account.subscribeToLottery)
            {
                subscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"subscribe_button", "Unsubscribe To Lottery", false);
            }
            else
            {
                subscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"subscribe_button", "Subscribe To Lottery", false);
            }

            DiscordButtonComponent setSafeBalance = new DiscordButtonComponent(DiscordButtonStyle.Primary, "setSafeBalance", "Set Safe Balance", false);
            DiscordButtonComponent setTicketAmount = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "setTicketAmount", "Set Ticket Amount", false);

            await ctx.DeferAsync(true);

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(settingsEmbed(ctx.User, account))
                .AddComponents(subscribeBtn, setSafeBalance, setTicketAmount).AsEphemeral(true));
        }

        #endregion Settings

        #region Beer

        [SlashCommand("beer", "Give a beer to Fitz")]
        [RequireAccount]
        public async Task GiveBeer(InteractionContext ctx, [Option("Beer", "How much beer do you want to give Fitz?", false)] double amount = 0)
        {
            var giveBeerCommand = new GiveBeerCommand(scopeFactory, botLog, bankService);
            var result = await giveBeerCommand.ExecuteAsync(ctx, amount);
            
            if (result.Success)
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent(result.Message).AsEphemeral(true));
            }
            else
            {
                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent(result.Message).AsEphemeral(true));
            }
        }

        #endregion Beer

        #region Embeds

        private DiscordEmbed accountEmbed(DiscordUser user, AccountEntity account)
        {
            List<Poll> userPolls = this.pollService.GetPollsSubmittedByUser(account.Id);
            List<Ticket> userTickets = this.lotteryService.GetTicketsByUserId(account.Id);

            string subscribe = account.subscribeToLottery ? "Active" : "Inactive";
            DiscordEmbedBuilder accountEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, AccountEmojis.Users).Url,
                    Text = $"Account Information",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = user.AvatarUrl,
                },
                Description = "I collect beer and stupid user data.\n" +
                $"Edit your account settings using `/settings`\n\n" +
                $"**Beer**: `{account.Beer}`\n" +
                $"**Lifetime Beer**: `{account.LifetimeBeer}`\n" +
                $"**Favorability**: `{account.Favorability}%`\n" +
                $"**Lottery Subscription**: `{subscribe}`\n" +
                $"**Safe Balance**: `{account.safeBalance}`"
            };

            accountEmbed.AddField($"**Polls**", $"Submitted: `{userPolls.Count()}`\n" +
                $"Approved: `{userPolls.Where(poll => poll.Status == PollStatus.Approved).Count()}`\n" +
                $"Pending: `{userPolls.Where(poll => poll.Status == PollStatus.Pending).Count()}`\n" +
                $"Declined: `{userPolls.Where(poll => poll.Status == PollStatus.Declined).Count()}`", true);

            accountEmbed.AddField($"**Lottery**", $"Partcipated: `{this.lotteryService.GetTotalLotteryPartipationsByUserId(account.Id)}`\n" +
                $"Lifetime Entries: `{userTickets.Count()}`\n" +
                $"Wins: `{this.lotteryService.GetTotalWinsByAccountId(account.Id)}`\n" +
                $"Largest Payout: `{this.lotteryService.GetLargestPayoutByUserId(account.Id)}`", true);

            accountEmbed.AddField($"**Renames**", $"Requests: `{this.renameService.GetTotalRenameRequestsByAccountId(account.Id)}`\n" +
                $"Renamed: `{this.renameService.GetTotalRenamesByAccountId(account.Id)}`\n" +
                $"Highest Cost: `WIP`\n", true);

            return accountEmbed.Build();
        }

        private DiscordEmbed settingsEmbed(DiscordUser user, AccountEntity account)
        {
            DiscordEmbedBuilder settingsEmbed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, AccountEmojis.Edit).Url,
                    Text = $"Account Settings | ID: {account.Id}",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = user.AvatarUrl,
                },
                Description = $"Change your account settings using the buttons below.\n"
            };

            if (account.subscribeToLottery)
            {
                settingsEmbed.AddField($"{DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Lottery)} __**Lottery Subscription**__: `Active` {DiscordEmoji.FromName(this.dClient, ":white_check_mark:", true)}", $"If active, Fitz will buy tickets for you each lottery.");
            }
            else
            {
                settingsEmbed.AddField($"{DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Lottery)} __**Lottery Subscription**__: `Inactive` {DiscordEmoji.FromName(this.dClient, ":x:", true)}", $"If active, Fitz will buy tickets for you each lottery.");
            }

            settingsEmbed.AddField($"{DiscordEmoji.FromName(this.dClient, ":beer:", true)} __**Safe Balance**__: {account.safeBalance}", $"The amount of money you want before you stop auto-entering the lottery.", false);
            settingsEmbed.AddField($"{DiscordEmoji.FromGuildEmote(this.dClient, LotteryEmojis.Ticket)} __**Tickets**__: {account.SubscribeTickets}", $"The number of tickets you want to buy each lottery.", false);

            return settingsEmbed.Build();
        }

        #endregion Embeds

        #region Events

        private async Task HandleAccountSettingsEvent(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            // Set the message this interaction is for.
            DiscordMessage accountSettingsMessage = args.Message;

            // Get the account who is interacting
            var findAccountQuery = new FindAccountQuery(scopeFactory);
            AccountEntity account = findAccountQuery.Execute(args.User.Id);

            // If the subscribe button was pressed.
            if (args.Id == $"subscribe_button" && args.User.Id == account.Id)
            {
                // Set their lottery subscription to the opposite of what it currently is.
                var setLotterySubscribeCommand = new SetLotterySubscribeCommand(scopeFactory, botLog);
                await setLotterySubscribeCommand.ExecuteAsync(account, !account.subscribeToLottery);

                // Retrieve Updated Account Settings
                account = findAccountQuery.Execute(account.Id);

                // Modify the original message.
                DiscordButtonComponent subscribeBtn;
                if (account.subscribeToLottery)
                {
                    subscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"subscribe_button", "Unsubscribe To Lottery", false);
                }
                else
                {
                    subscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"subscribe_button", "Subscribe To Lottery", false);
                }
                DiscordButtonComponent setSafeBalance = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "setSafeBalance", "Set Safe Balance", false);
                DiscordButtonComponent setTicketAmount = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "setTicketAmount", "Set Ticket Amount", false);

                await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                    .AddEmbed(settingsEmbed(args.User, account)).AddComponents(subscribeBtn, setSafeBalance, setTicketAmount).AsEphemeral(true));
                return;
            }
            if (args.Id == $"setSafeBalance" && args.User.Id == account.Id)
            {
                var numberModal = ModalBuilder.Create("set_safe_balance")
                .WithTitle("Set Safe Balance")
                .AddComponents(new DiscordTextInputComponent("Safe Balance", "safe_balance", "Safe Balance", required: true, max_length: 11));

                await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, numberModal);

                this.dClient.ModalSubmitted += async (dClientModal, modalSubmitEvent) =>
                {
                    #region Account Settings - Modal - Safe Balance

                    if (modalSubmitEvent.Values.ContainsKey($"safe_balance") && modalSubmitEvent.Interaction.User.Id == account.Id)
                    {
                        int safeBalance = int.Parse(modalSubmitEvent.Values["safe_balance"]);
                        var setSafeBalanceCommand = new SetSafeBalanceCommand(scopeFactory, botLog);
                        var setSafeBalanceResult = await setSafeBalanceCommand.ExecuteAsync(account, safeBalance);
                        if (setSafeBalanceResult.Success)
                        {
                            account = findAccountQuery.Execute(account.Id);
                            DiscordButtonComponent subscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "subscribe_button", "Subscribe To Lottery", false);
                            DiscordButtonComponent setSafeBalance = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "setSafeBalance", "Set Safe Balance", false);
                            DiscordButtonComponent setTicketAmount = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "setTicketAmount", "Set Ticket Amount", false);
                            await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                                                           .AddEmbed(settingsEmbed(args.User, account))
                                                           .AddComponents(subscribeBtn, setSafeBalance, setTicketAmount).WithContent("Updated your safe balance."));
                        }
                    }

                    #endregion Account Settings - Modal - Safe Balance
                };

                return;
            }
            if (args.Id == "setTicketAmount" && args.User.Id == account.Id)
            {
                var ticketModal = ModalBuilder.Create("set_ticket_amount")
                .WithTitle("Set Ticket Amount")
                .AddComponents(new DiscordTextInputComponent("Tickets", "safe_tickets", "Tickets", required: true, max_length: 11));

                await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, ticketModal);

                this.dClient.ModalSubmitted += async (dClientModal, modalSubmitEvent) =>
                {
                    #region Account Settings - Modal - Safe Balance

                    if (modalSubmitEvent.Values.ContainsKey($"safe_tickets") && modalSubmitEvent.Interaction.User.Id == account.Id)
                    {
                        int ticketAmount = int.Parse(modalSubmitEvent.Values["safe_tickets"]);
                        var setTicketAmountCommand = new SetTicketAmountCommand(scopeFactory, botLog);
                        var setTicketAmountResult = await setTicketAmountCommand.ExecuteAsync(account, ticketAmount);
                        if (setTicketAmountResult.Success)
                        {
                            account = findAccountQuery.Execute(account.Id);
                            DiscordButtonComponent subscribeBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "subscribe_button", "Subscribe To Lottery", false);
                            DiscordButtonComponent setSafeBalance = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "setSafeBalance", "Set Safe Balance", false);
                            DiscordButtonComponent setTicketAmount = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "setTicketAmount", "Set Ticket Amount", false);
                            await args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                                                           .AddEmbed(settingsEmbed(args.User, account))
                                                           .AddComponents(subscribeBtn, setSafeBalance, setTicketAmount).WithContent("Updated your safe ticket amount."));
                        }
                    }

                    #endregion Account Settings - Modal - Safe Balance
                };

                return;
            }
        }

        #endregion Events
    }
}