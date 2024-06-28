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
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Blackjack.Modals;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Fitz.Features.Blackjack
{
    public sealed class BlackJackService(IServiceScopeFactory scopeFactory, BotLog botLog, AccountService accountService, BankService bankService)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;
        private readonly AccountService accountService = accountService;
        private readonly BankService bankService = bankService;

        #region Start New Blackjack Game

        public async Task<Result> StartNewBlackjackGameAsync(GameType type, DiscordMessage message)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Deck deck = new Deck();

                deck.Shuffle();

                deck.Shuffle();

                deck.Shuffle();

                var deckJson = JsonSerializer.Serialize(deck.cards).ToString();

                BlackjackGame blackjackGame = new BlackjackGame
                {
                    Jackpot = 0,
                    Status = BlackjackGameStatus.Unknown,
                    MessageId = message.Id,
                    Type = type,
                    Started = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    Ended = null,
                    Dealer = this.accountService.FindAccount(Users.Fitz),
                    DeckJson = deckJson,
                };

                db.BlackjackGame.Add(blackjackGame);
                await db.SaveChangesAsync();

                return new Result(true, "Started new blackjack game.", blackjackGame);
            }
            catch (Exception ex)
            {
                botLog.Information(LogConsoleSettings.LotteryLog, BlackjackEmojis.Stand, "failed to create new blackjack game");
                return new Result(false, $"An error occurred while starting a new blackjack game. {ex.Message}", null);
            }
        }

        #endregion Start New Blackjack Game

        #region Add Player to Blackjack Game

        public async Task<Result> AddPlayerToGameAsync(BlackjackGame game, Account account)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                BlackjackPlayers player = new BlackjackPlayers
                {
                    GameId = game.Id,
                    UserId = account.Id,
                    Bet = 0,
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

            game.Players = db.BlackjackPlayers.Where(player => player.GameId == game.Id).ToList();

            game.Deck = JsonSerializer.Deserialize<Deck>(game.DeckJson);

            return game;
        }

        #endregion Get Blackjack Game

        #region Deal Cards

        public BlackjackGame Deal(BlackjackGame game)
        {
            game.Deck = JsonSerializer.Deserialize<Deck>(game.DeckJson);

            if (game.Deck.Cards.Count < 2)
            {
                // Can't continue, not enough cards.
            }

            foreach (BlackjackPlayers player in game.Players)
            {
                var card = game.Deck.Cards.FirstOrDefault();
                player.Hand.AddCard(card);
                game.Deck.cards.Remove(card);

                card = game.Deck.cards.FirstOrDefault();
                player.Hand.AddCard(card);
                game.Deck.cards.Remove(card);
            }
            return game;
        }

        public void Hit(BlackjackGame game, Account playerAccount)
        {
            BlackjackPlayers player = game.Players.FirstOrDefault(p => p.Account.Id == playerAccount.Id);
            var card = game.Deck.cards.FirstOrDefault();
            player.Hand.AddCard(card);
            game.Deck.cards.Remove(card);

            if (PlayerHasBusted(game, playerAccount))
            {
                player.IsWinner = false;
            }
        }

        private void Stand(BlackjackGame game, Account playerAccount)
        {
            BlackjackPlayers player = game.Players.FirstOrDefault(p => p.Account.Id == playerAccount.Id);
        }

        private bool PlayerHasBusted(BlackjackGame game, Account playerAccount)
        {
            BlackjackPlayers player = game.Players.FirstOrDefault(p => p.Account.Id == playerAccount.Id);

            if (player.Hand.TotalValue > 21)
            {
                player.IsBusted = true;
                return true;
            }
            return false;
        }

        #endregion Deal Cards
    }
}