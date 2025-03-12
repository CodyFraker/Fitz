using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Blackjack.Modals;
using Fitz.Variables.Emojis;
using System.Linq;
using System.Threading.Tasks;
using AccountModel = Fitz.Features.Accounts.Models.Account;
using System.Text;

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
                if (game == null)
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                        new DiscordInteractionResponseBuilder().WithContent("Game not found.").AsEphemeral(true));
                    return;
                }

                DiscordMessage gameMessage = await this.dClient.GetChannelAsync(args.Channel.Id).Result.GetMessageAsync(game.MessageId);

                if (game.Players.Any(x => x.UserId == args.User.Id))
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                        new DiscordInteractionResponseBuilder().WithContent("You are already in the game.").AsEphemeral(true));
                    return;
                }

                AccountModel newPlayer = accountService.FindAccount(args.User.Id);
                if (newPlayer == null)
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                        new DiscordInteractionResponseBuilder().WithContent("You need an account to play.").AsEphemeral(true));
                    return;
                }

                int bet = 0;
                switch (game.Type)
                {
                    case GameType.Normal:
                        bet = 12;
                        break;
                    case GameType.HighStakes:
                        bet = 36;
                        break;
                    case GameType.AllOrNothing:
                        var balance = await bankService.GetBalanceAsync(newPlayer.Id);
                        bet = balance;
                        break;
                }

                var addPlayerResult = await this.blackJackService.AddPlayerToGameAsync(game, newPlayer, bet);
                if (!addPlayerResult.Success)
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                        new DiscordInteractionResponseBuilder().WithContent(addPlayerResult.Message).AsEphemeral(true));
                    return;
                }

                game = (BlackjackGame)addPlayerResult.Data;

                DiscordButtonComponent joinBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "blackjack_join", "Join", false);
                DiscordButtonComponent startBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_start", "Start", false);
                DiscordButtonComponent hitBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "blackjack_hit", "Hit", true);
                DiscordButtonComponent standBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "blackjack_stand", "Stand", true);

                await gameMessage.ModifyAsync(new DiscordMessageBuilder()
                    .WithContent($"{newPlayer.Username} has joined the game.")
                    .AddComponents(joinBtn, startBtn)
                    .AddEmbed(blackJackEmbed(game)));

                await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            }
            else if (args.Id == "blackjack_start")
            {
                BlackjackGame game = this.blackJackService.GetBlackjackGame(args.Message.Id);
                if (game == null)
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                        new DiscordInteractionResponseBuilder().WithContent("Game not found.").AsEphemeral(true));
                    return;
                }

                if (game.Players.Count < 2)
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                        new DiscordInteractionResponseBuilder().WithContent("Need at least one player to start.").AsEphemeral(true));
                    return;
                }

                DiscordMessage gameMessage = await this.dClient.GetChannelAsync(args.Channel.Id).Result.GetMessageAsync(game.MessageId);
                game = this.blackJackService.Deal(game);

                DiscordButtonComponent hitBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "blackjack_hit", "Hit", false);
                DiscordButtonComponent standBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "blackjack_stand", "Stand", false);

                await gameMessage.ModifyAsync(new DiscordMessageBuilder()
                    .WithContent("Cards have been dealt.")
                    .AddComponents(hitBtn, standBtn)
                    .AddEmbed(InProgressBlackjackEmbed(game)));

                await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            }
            else if (args.Id == "blackjack_hit")
            {
                BlackjackGame game = this.blackJackService.GetBlackjackGame(args.Message.Id);
                if (game == null)
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                        new DiscordInteractionResponseBuilder().WithContent("Game not found.").AsEphemeral(true));
                    return;
                }

                AccountModel player = accountService.FindAccount(args.User.Id);
                var hitResult = await this.blackJackService.Hit(game, player);
                if (!hitResult.Success)
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                        new DiscordInteractionResponseBuilder().WithContent(hitResult.Message).AsEphemeral(true));
                    return;
                }

                game = (BlackjackGame)hitResult.Data;
                DiscordMessage gameMessage = await this.dClient.GetChannelAsync(args.Channel.Id).Result.GetMessageAsync(game.MessageId);

                DiscordButtonComponent hitBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "blackjack_hit", "Hit", false);
                DiscordButtonComponent standBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "blackjack_stand", "Stand", false);

                var messageBuilder = new DiscordMessageBuilder()
                    .AddEmbed(InProgressBlackjackEmbed(game));

                if (game.Status != BlackjackGameStatus.Ended)
                {
                    messageBuilder.AddComponents(hitBtn, standBtn);
                }

                await gameMessage.ModifyAsync(messageBuilder);
                await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            }
            else if (args.Id == "blackjack_stand")
            {
                BlackjackGame game = this.blackJackService.GetBlackjackGame(args.Message.Id);
                if (game == null)
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                        new DiscordInteractionResponseBuilder().WithContent("Game not found.").AsEphemeral(true));
                    return;
                }

                AccountModel player = accountService.FindAccount(args.User.Id);
                var standResult = await this.blackJackService.Stand(game, player);
                if (!standResult.Success)
                {
                    await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                        new DiscordInteractionResponseBuilder().WithContent(standResult.Message).AsEphemeral(true));
                    return;
                }

                game = (BlackjackGame)standResult.Data;
                DiscordMessage gameMessage = await this.dClient.GetChannelAsync(args.Channel.Id).Result.GetMessageAsync(game.MessageId);

                DiscordButtonComponent hitBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "blackjack_hit", "Hit", false);
                DiscordButtonComponent standBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "blackjack_stand", "Stand", false);

                var messageBuilder = new DiscordMessageBuilder()
                    .AddEmbed(InProgressBlackjackEmbed(game));

                if (game.Status != BlackjackGameStatus.Ended)
                {
                    messageBuilder.AddComponents(hitBtn, standBtn);
                }

                await gameMessage.ModifyAsync(messageBuilder);
                await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            }
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

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .AddEmbed(PreGameBlackjackEmbed())
                .AddComponents(noStakes, normalBtn, highstakesBtn, allOrNothingBtn)
                .AsEphemeral(true));

            ctx.Client.ComponentInteractionCreated += async (sender, args) =>
            {
                if (args.User.IsBot || args.User.Id != ctx.User.Id)
                {
                    return;
                }

                DiscordButtonComponent joinBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "blackjack_join", "Join", false);
                DiscordButtonComponent startBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_start", "Start", false);

                GameType gameType = GameType.NoStakes;
                string gameTypeStr = "No Stakes";

                if (args.Id == "noStakes")
                {
                    gameType = GameType.NoStakes;
                    gameTypeStr = "No Stakes";
                }
                else if (args.Id == "normal")
                {
                    gameType = GameType.Normal;
                    gameTypeStr = "Normal";
                }
                else if (args.Id == "highstakes")
                {
                    gameType = GameType.HighStakes;
                    gameTypeStr = "High Stakes";
                }
                else if (args.Id == "allOrNothing")
                {
                    gameType = GameType.AllOrNothing;
                    gameTypeStr = "All or Nothing";
                }
                else
                {
                    return;
                }

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Starting {gameTypeStr} Blackjack Game..."));

                DiscordMessage blackJackmessage = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent("Starting blackjack!"));

                var startGameResult = await this.blackJackService.StartNewBlackjackGameAsync(gameType, blackJackmessage);

                if (!startGameResult.Success)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Failed to create new blackjack instance: {startGameResult.Message}"));
                    return;
                }

                BlackjackGame game = (BlackjackGame)startGameResult.Data;

                await blackJackmessage.ModifyAsync(new DiscordMessageBuilder()
                    .AddEmbed(blackJackEmbed(game))
                    .AddComponents(joinBtn, startBtn));
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
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, BlackjackEmojis.Cards).Url,
                    Text = $"Blackjack | Choose your game type",
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
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, BlackjackEmojis.Cards).Url,
                    Text = $"Blackjack | Waiting for players",
                },
                Color = new DiscordColor(52, 114, 53),
                Title = $"Starting new blackjack game",
                Description = $"Game Type: {game.Type}\n" +
                            (game.Type != GameType.NoStakes ? $"Bet Amount: {GetBetAmount(game.Type)}\n" : "") +
                            $"Waiting for others to join..",
            };

            string playerNames = string.Empty;
            if (game.Players != null && game.Players.Count > 0)
            {
                foreach (BlackjackPlayers player in game.Players.Where(p => !p.IsDealer))
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
            BlackjackPlayers dealer = game.Players.FirstOrDefault(x => x.IsDealer);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, BlackjackEmojis.Cards).Url,
                    Text = game.Status == BlackjackGameStatus.Ended ? "Blackjack | Game Over" : "Blackjack | In Progress",
                },
                Color = new DiscordColor(52, 114, 53),
                Title = $"Blackjack",
                Description = $"Game Type: {game.Type}\n" +
                            (game.Type != GameType.NoStakes ? $"Bet Amount: {GetBetAmount(game.Type)}\n" : "") +
                            $"Dealer: {game.Dealer.Username}\n",
            };

            // Show dealer's hand
            string dealerHand = FormatHand(dealer.Hand);
            string dealerTitle = game.Status == BlackjackGameStatus.Ended ? 
                $"Dealer's Hand: {dealer.Hand.TotalValue}" : 
                $"Dealer's Hand: {dealer.Hand.FaceValue}";
            embed.AddField(dealerTitle, dealerHand);

            // Show each player's hand
            foreach (BlackjackPlayers player in game.Players.Where(p => !p.IsDealer))
            {
                string playerHand = FormatHand(player.Hand);
                string fieldTitle;

                if (player.IsBusted)
                {
                    fieldTitle = $"~~{player.Account.Username}'s Hand: {player.Hand.TotalValue}~~ BUST";
                }
                else if (game.Status == BlackjackGameStatus.Ended)
                {
                    fieldTitle = player.IsWinner ? 
                        $"{player.Account.Username}'s Hand: {player.Hand.TotalValue} WINNER!" :
                        $"{player.Account.Username}'s Hand: {player.Hand.TotalValue}";
                }
                else
                {
                    fieldTitle = player.HasTurn ?
                        $"➡️ {player.Account.Username}'s Hand: {player.Hand.TotalValue}" :
                        $"{player.Account.Username}'s Hand: {player.Hand.TotalValue}";
                }

                embed.AddField(fieldTitle, playerHand);
            }

            return embed.Build();
        }

        private string FormatHand(Hand hand)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Card card in hand.Cards)
            {
                if (!card.IsFaceUp && !hand.IsDealer)
                {
                    sb.AppendLine("🎴");
                    continue;
                }

                string cardStr = card.IsFaceUp ? GetCardString(card) : "🎴";
                sb.AppendLine(cardStr);
            }
            return sb.ToString();
        }

        private string GetCardString(Card card)
        {
            string rankStr = card.Rank switch
            {
                Rank.Ace => "A",
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                _ => ((int)card.Rank).ToString()
            };

            string suitEmoji = card.Suite switch
            {
                Suite.Spades => "♠️",
                Suite.Heart => "♥️",
                Suite.Diamond => "♦️",
                Suite.Club => "♣️",
                _ => "?"
            };

            return $"{rankStr}{suitEmoji}";
        }

        private int GetBetAmount(GameType type)
        {
            return type switch
            {
                GameType.Normal => 12,
                GameType.HighStakes => 36,
                GameType.AllOrNothing => 0, // This will be calculated per player
                _ => 0
            };
        }

        #endregion Embeds
    }
}