using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Bank.AddBalance.Domain;
using Fitz.Features.Bank.Models;
using Fitz.Features.Blackjack.Modals;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using AccountModel = Fitz.Features.Accounts.Models.Account;

namespace Fitz.Features.Blackjack
{
    public sealed class BlackJackService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly BotLog botLog;
        private readonly AccountService accountService;
        private readonly AddBalanceService addBalanceService;
        private const int GAME_TIMEOUT_MINUTES = 5;
        private const int DEALER_STAND_VALUE = 17;

        public BlackJackService(
            IServiceScopeFactory scopeFactory, 
            BotLog botLog, 
            AccountService accountService,
            AddBalanceService addBalanceService)
        {
            this.scopeFactory = scopeFactory;
            this.botLog = botLog;
            this.accountService = accountService;
            this.addBalanceService = addBalanceService;
        }

        #region Start New Blackjack Game

        public async Task<Result> StartNewBlackjackGameAsync(GameType type, DiscordMessage message)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var dealer = this.accountService.FindAccount(Users.Fitz);
                if (dealer == null)
                {
                    return new Result(false, "Failed to find dealer account.", null);
                }

                BlackjackGame blackjackGame = new BlackjackGame
                {
                    Jackpot = 0,
                    Status = BlackjackGameStatus.InProgress,
                    MessageId = message.Id,
                    Type = type,
                    Started = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    Ended = null,
                };

                blackjackGame.InitializeDealer(dealer);
                blackjackGame.Deck.Shuffle();
                blackjackGame.DeckJson = JsonSerializer.Serialize(blackjackGame.Deck.Cards).ToString();

                db.BlackjackGame.Add(blackjackGame);
                await db.SaveChangesAsync();

                // Start timeout checker
                _ = Task.Run(async () => await CheckGameTimeout(blackjackGame.Id));

                return new Result(true, "Started new blackjack game.", blackjackGame);
            }
            catch (Exception ex)
            {
                botLog.Information(LogConsoleSettings.LotteryLog, BlackjackEmojis.Stand, "failed to create new blackjack game");
                return new Result(false, $"An error occurred while starting a new blackjack game. {ex.Message}", null);
            }
        }

        private async Task CheckGameTimeout(int gameId)
        {
            await Task.Delay(TimeSpan.FromMinutes(GAME_TIMEOUT_MINUTES));
            
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            var game = db.BlackjackGame.FirstOrDefault(g => g.Id == gameId);
            if (game != null && game.Status == BlackjackGameStatus.InProgress)
            {
                var timeSinceLastUpdate = DateTime.UtcNow - game.Updated;
                if (timeSinceLastUpdate.TotalMinutes >= GAME_TIMEOUT_MINUTES)
                {
                    game.Status = BlackjackGameStatus.Stale;
                    game.Ended = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                    
                    // Return bets to players if any
                    if (game.Type != GameType.NoStakes)
                    {
                        var players = db.BlackjackPlayers.Where(p => p.GameId == gameId).ToList();
                        foreach (var player in players)
                        {
                            var command = new AddBalanceCommand(
                                recipientId: player.UserId,
                                senderId: Users.Fitz,
                                amount: player.Bet,
                                reason: TransactionReason.GameRefund,
                                updateLifetimeBalance: false);
                            await addBalanceService.AddBalanceAsync(command);
                        }
                    }
                }
            }
        }

        #endregion Start New Blackjack Game

        #region Add Player to Blackjack Game

        public async Task<Result> AddPlayerToGameAsync(BlackjackGame game, AccountModel account, int bet = 0)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                // If game has stakes, deduct the bet
                if (game.Type != GameType.NoStakes && bet > 0)
                {
                    var deductCommand = new DeductBalanceCommand(
                        userId: account.Id,
                        amount: bet,
                        reason: TransactionReason.GameBet);
                    
                    var deductResult = await addBalanceService.DeductBalanceAsync(deductCommand);
                    if (!deductResult.Success)
                    {
                        return new Result(false, "Failed to place bet: " + deductResult.Message, null);
                    }
                }

                BlackjackPlayers player = new BlackjackPlayers
                {
                    GameId = game.Id,
                    UserId = account.Id,
                    Bet = bet,
                    HasTurn = true,
                    IsDealer = false,
                    IsWinner = false,
                    IsBusted = false,
                    Account = account,
                };

                game.Updated = DateTime.UtcNow;
                db.BlackjackGame.Update(game);
                db.BlackjackPlayers.Add(player);

                await db.SaveChangesAsync();
                game.Players.Add(player);

                return new Result(true, "Added player to blackjack game.", game);
            }
            catch (Exception ex)
            {
                botLog.Information(LogConsoleSettings.LotteryLog, BlackjackEmojis.Stand, "failed to add player to blackjack game");
                return new Result(false, $"An error occurred while adding a player to the blackjack game. {ex.Message}", null);
            }
        }

        #endregion Add Player to Blackjack Game

        #region Get Blackjack Game

        public BlackjackGame GetBlackjackGame(ulong messageId)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            BlackjackGame game = db.BlackjackGame.FirstOrDefault(blackjack => blackjack.MessageId == messageId);
            if (game == null) return null;

            game.Players = db.BlackjackPlayers.Where(player => player.GameId == game.Id).ToList();
            game.Deck = JsonSerializer.Deserialize<Deck>(game.DeckJson);

            // Set the dealer reference
            var dealer = game.Players.FirstOrDefault(p => p.IsDealer);
            if (dealer != null)
            {
                game.Dealer = dealer.Account;
            }

            return game;
        }

        #endregion Get Blackjack Game

        #region Deal Cards

        public BlackjackGame Deal(BlackjackGame game)
        {
            if (game.Deck.Cards.Count < 2 * (game.Players.Count + 1)) // +1 for dealer
            {
                throw new InvalidOperationException("Not enough cards to deal to all players");
            }

            // Deal to players first
            foreach (BlackjackPlayers player in game.Players.Where(p => !p.IsDealer))
            {
                game.Deck.Deal(player.Hand);
            }

            // Deal to dealer last
            var dealer = game.Players.FirstOrDefault(p => p.IsDealer);
            if (dealer != null)
            {
                game.Deck.Deal(dealer.Hand);
            }

            // Update game state
            game.Status = BlackjackGameStatus.InProgress;
            game.Updated = DateTime.UtcNow;

            // Serialize updated deck back to JSON
            game.DeckJson = JsonSerializer.Serialize(game.Deck.Cards).ToString();

            return game;
        }

        public async Task<Result> Hit(BlackjackGame game, AccountModel playerAccount)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                BlackjackPlayers player = game.Players.FirstOrDefault(p => p.Account.Id == playerAccount.Id);
                if (player == null || !player.HasTurn)
                {
                    return new Result(false, "It's not your turn.", null);
                }

                game.Deck.GiveAdditionalCard(player.Hand);

                if (PlayerHasBusted(game, playerAccount))
                {
                    player.IsWinner = false;
                    player.IsBusted = true;
                    player.HasTurn = false;
                    await NextTurn(game);
                }

                game.Updated = DateTime.UtcNow;
                game.DeckJson = JsonSerializer.Serialize(game.Deck.Cards).ToString();
                db.BlackjackGame.Update(game);
                await db.SaveChangesAsync();

                return new Result(true, "Hit successful.", game);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Error during hit: {ex.Message}", null);
            }
        }

        public async Task<Result> Stand(BlackjackGame game, AccountModel playerAccount)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                BlackjackPlayers player = game.Players.FirstOrDefault(p => p.Account.Id == playerAccount.Id);
                if (player == null || !player.HasTurn)
                {
                    return new Result(false, "It's not your turn.", null);
                }

                player.HasTurn = false;
                await NextTurn(game);

                game.Updated = DateTime.UtcNow;
                db.BlackjackGame.Update(game);
                await db.SaveChangesAsync();

                return new Result(true, "Stand successful.", game);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Error during stand: {ex.Message}", null);
            }
        }

        private async Task NextTurn(BlackjackGame game)
        {
            var nextPlayer = game.Players.FirstOrDefault(p => !p.IsDealer && !p.IsBusted && p.HasTurn);
            
            if (nextPlayer == null)
            {
                // All players have finished their turns, dealer's turn
                await DealerTurn(game);
            }
        }

        private async Task DealerTurn(BlackjackGame game)
        {
            var dealer = game.Players.FirstOrDefault(p => p.IsDealer);
            if (dealer == null) return;

            // Show dealer's face-down cards
            dealer.Hand.Show();

            // Dealer must hit on 16 or below, stand on 17 or above
            while (dealer.Hand.TotalValue < DEALER_STAND_VALUE)
            {
                game.Deck.GiveAdditionalCard(dealer.Hand);
            }

            await DetermineWinners(game);
        }

        private async Task DetermineWinners(BlackjackGame game)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            var dealer = game.Players.FirstOrDefault(p => p.IsDealer);
            var dealerValue = dealer?.Hand.TotalValue ?? 0;
            bool dealerBusted = dealerValue > 21;

            foreach (var player in game.Players.Where(p => !p.IsDealer && !p.IsBusted))
            {
                if (dealerBusted || player.Hand.TotalValue > dealerValue)
                {
                    player.IsWinner = true;
                    if (game.Type != GameType.NoStakes)
                    {
                        var winCommand = new AddBalanceCommand(
                            recipientId: player.UserId,
                            senderId: Users.Fitz,
                            amount: player.Bet * 2, // Return bet + winnings
                            reason: TransactionReason.GameWin,
                            updateLifetimeBalance: true);
                        await addBalanceService.AddBalanceAsync(winCommand);
                    }
                }
                else if (player.Hand.TotalValue == dealerValue)
                {
                    // Push - return original bet
                    if (game.Type != GameType.NoStakes)
                    {
                        var pushCommand = new AddBalanceCommand(
                            recipientId: player.UserId,
                            senderId: Users.Fitz,
                            amount: player.Bet,
                            reason: TransactionReason.GameRefund,
                            updateLifetimeBalance: false);
                        await addBalanceService.AddBalanceAsync(pushCommand);
                    }
                }
            }

            game.Status = BlackjackGameStatus.Ended;
            game.Ended = DateTime.UtcNow;
            game.Updated = DateTime.UtcNow;
            
            db.BlackjackGame.Update(game);
            await db.SaveChangesAsync();
        }

        private bool PlayerHasBusted(BlackjackGame game, AccountModel playerAccount)
        {
            BlackjackPlayers player = game.Players.FirstOrDefault(p => p.Account.Id == playerAccount.Id);
            return player?.Hand.TotalValue > 21;
        }

        #endregion Deal Cards
    }
}