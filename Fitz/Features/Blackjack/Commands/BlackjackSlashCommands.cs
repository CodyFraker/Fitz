using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Blackjack.Modals;
using Fitz.Variables.Emojis;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Blackjack.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class BlackjackSlashCommands : ApplicationCommandModule
    {
        private readonly DiscordClient dClient;
        private readonly AccountService accountService;
        private readonly BankService bankService;
        private readonly BlackJackService blackJackService;

        public BlackjackSlashCommands(DiscordClient dClient, AccountService accountService, BankService bankService, BlackJackService blackJackService)
        {
            this.dClient = dClient;
            this.accountService = accountService;
            this.bankService = bankService;
            this.blackJackService = blackJackService;

            this.dClient.ComponentInteractionCreated += ModifyBlackjack;
        }

        private async Task ModifyBlackjack(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            if (args.User.IsBot)
            {
                return;
            }

            if (args.Id == "blackjack_join")
            {
                BlackjackGame game = this.blackJackService.GetBlackjackGame(args.Message.Id);

                DiscordMessage gameMessage = await this.dClient.GetChannelAsync(args.Channel.Id).Result.GetMessageAsync(game.MessageId);

                if (game.Players.Contains(game.Players.Find(x => x.UserId == args.User.Id)))
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are already in the game.").AsEphemeral(true));
                    return;
                }
                else
                {
                    Account newPlayer = accountService.FindAccount(args.User.Id);
                    var addPlayerResult = await this.blackJackService.AddPlayerToGameAsync(game, newPlayer);

                    if (addPlayerResult.Success)
                    {
                        game = (BlackjackGame)addPlayerResult.Data;
                    }

                    DiscordButtonComponent joinBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "blackjack_join", "Join", false);
                    DiscordButtonComponent startBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_start", "Start", false);

                    await gameMessage.ModifyAsync(new DiscordMessageBuilder()
                        .ClearEmbeds()
                        .WithContent($"{newPlayer.Username} has joined the game.")
                        .AddComponents(joinBtn, startBtn)
                        .AddEmbed(blackJackEmbed(game)));
                }
            }
            if (args.Id == "blackjack_start")
            {
                BlackjackGame game = this.blackJackService.GetBlackjackGame(args.Message.Id);

                DiscordMessage gameMessage = await this.dClient.GetChannelAsync(args.Channel.Id).Result.GetMessageAsync(game.MessageId);
                game = this.blackJackService.Deal(game);

                await gameMessage.ModifyAsync(new DiscordMessageBuilder()
                    .ClearEmbeds()
                    .WithContent($"Cards have been delt.")
                    .AddEmbed(InProgressBlackjackEmbed(game)));
            }
            // Send a bad request to trick discord into thinking there was an interaction made to the message.
            await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("").AsEphemeral(true));
        }

        #region Command

        [SlashCommand("blackjack", "Start a game of blackjack")]
        [RequireAccount]
        public async Task BlackJack(InteractionContext ctx)
        {
            DiscordButtonComponent noStakes = new DiscordButtonComponent(DiscordButtonStyle.Primary, "noStakes", "No Stakes", false);
            DiscordButtonComponent normalBtn = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "normal", "Normal", false);
            DiscordButtonComponent highstakesBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "highstakes", "High Stakes", false);
            DiscordButtonComponent allOrNothingBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "allOrNothing", "All or Nothing", false);

            //await ctx.DeferAsync(true);
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(PreGameBlackjackEmbed())
                .AddComponents(noStakes, normalBtn, highstakesBtn, allOrNothingBtn).AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (sender, args) =>
            {
                if (args.User.IsBot)
                {
                    return;
                }

                DiscordButtonComponent joinBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "blackjack_join", "Join", false);
                DiscordButtonComponent startBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_Start", "Start", false);

                if (args.Id == "noStakes")
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Starting No Stakes Blackjack Game..."));

                    DiscordMessage blackJackmessage = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent("Starting blackjack!"));

                    var startGameResult = await this.blackJackService.StartNewBlackjackGameAsync(GameType.NoStakes, blackJackmessage);

                    if (!startGameResult.Success)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Failed to create new blackjack instance."));
                        return;
                    }
                    else
                    {
                        BlackjackGame game = (BlackjackGame)startGameResult.Data;

                        await blackJackmessage.ModifyAsync(new DiscordMessageBuilder()
                            .AddEmbed(blackJackEmbed(game))
                            .AddComponents(joinBtn, startBtn));
                    }
                }
                if (args.Id == "normal")
                {
                }
                if (args.Id == "highstakes")
                {
                }
                if (args.Id == "allOrNothing")
                {
                }
            };
        }

        #endregion Command

        #region Embeds

        private DiscordEmbed PreGameBlackjackEmbed()
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    //IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
                    Text = $"Blackjack #1 | Last Winner: Fitz",
                },
                Color = new DiscordColor(52, 114, 53),
                Title = $"What kind of blackjack are you feeling?",
                Description = $"No Stakes: No bets\n" +
                $"Normal: 12 Beer Bets\n" +
                $"High Stakes: 36 Beer Bets\n" +
                $"All or Nothing: All of your beer is bet\n\n" +
                $"Use the buttons below to make your choice.",
            };

            return embed.Build();
        }

        private DiscordEmbed blackJackEmbed(BlackjackGame game)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    //IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
                    Text = $"Blackjack #1 | Last Winner: Fitz",
                },
                Color = new DiscordColor(52, 114, 53),
                Title = $"Starting new blackjack game",
                Description = $"Waiting for others to join..\n",
            };

            string playerNames = string.Empty;
            if (game.Players != null && game.Players.Count > 0)
            {
                foreach (BlackjackPlayers player in game.Players)
                {
                    playerNames += $"{player.Account.Username} |{DiscordEmoji.FromGuildEmote(dClient, PollEmojis.Yes)}\n";
                }
            }
            else
            {
                playerNames = "No players have joined yet.";
            }
            embed.AddField($"Players", playerNames);

            return embed.Build();
        }

        private DiscordEmbed InProgressBlackjackEmbed(BlackjackGame game)
        {
            BlackjackPlayers dealer = game.Players.FirstOrDefault(x => x.IsDealer == true);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, BlackjackEmojis.Cards).Url,
                    Text = $"Blackjack #1 | Last Winner: ???",
                },
                Color = new DiscordColor(52, 114, 53),
                Title = $"Blackjack",
                Description = $"Dealer: {game.Dealer.Username}\n" +
                $"Beer Pool: 128\n",
            };

            string dealerHand = string.Empty;

            foreach (Card card in dealer.Hand.Cards)
            {
                switch (card.Suite)
                {
                    case Suite.Spades:
                        dealerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":spades:")}\n";
                        break;

                    case Suite.Diamond:
                        dealerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":diamonds:")}\n";
                        break;

                    case Suite.Club:
                        dealerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":clubs:")}\n";
                        break;

                    case Suite.Heart:
                        dealerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":heart:")}\n";
                        break;
                }
            }

            //if (game.LastState == GameState.DealerWon)
            //{
            //    embed.AddField($"{DiscordEmoji.FromName(dClient, ":house:")}Dealer Hand: {game.Dealer.Hand.TotalValue}{DiscordEmoji.FromName(dClient, ":star:")}", dealerHand);
            //}
            //else
            //{
            //    if (game.LastState == GameState.PlayerWon)
            //    {
            //        embed.AddField($"{DiscordEmoji.FromName(dClient, ":house:")}~~Dealer Hand: {game.Dealer.Hand.TotalValue}~~", dealerHand);
            //    }
            //    else
            //    {
            //        embed.AddField($"{DiscordEmoji.FromName(dClient, ":house:")}Dealer Hand: {game.Dealer.Hand.TotalValue}", dealerHand);
            //    }
            //}

            foreach (BlackjackPlayers player in game.Players)
            {
                string playerHand = string.Empty;
                foreach (Card card in player.Hand.Cards)
                {
                    switch (card.Suite)
                    {
                        case Suite.Spades:
                            playerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":spades:")}\n";
                            break;

                        case Suite.Diamond:
                            playerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":diamonds:")}\n";
                            break;

                        case Suite.Club:
                            playerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":clubs:")}\n";
                            break;

                        case Suite.Heart:
                            playerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":heart:")}\n";
                            break;
                    }
                }
                if (player.IsBusted)
                {
                    embed.AddField($"~~{player.Account.Username}'s Hand: {player.Hand.TotalValue}~~{DiscordEmoji.FromName(dClient, ":x:")}", playerHand);
                }
                else
                {
                    if (player.HasTurn)
                    {
                        embed.AddField($"{DiscordEmoji.FromName(dClient, ":arrow_right:")}{player.Account.Username}'s Hand: {player.Hand.TotalValue}", playerHand);
                    }
                    else
                    {
                        if (!player.IsBusted)
                        {
                            embed.AddField($"{player.Account.Username}'s Hand: {player.Hand.TotalValue}{DiscordEmoji.FromName(dClient, ":star:")}", playerHand);
                        }
                        else
                        {
                            embed.AddField($"{player.Account.Username}'s Hand: {player.Hand.TotalValue}", playerHand);
                        }
                    }
                }
            }

            return embed.Build();
        }

        #endregion Embeds

        //private bool isPlayerInGame(ulong id)
        //{
        //    return Players.Exists(x => x.Account.Id == id);
        //}
    }
}