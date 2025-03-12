using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AccountModel = Fitz.Features.Accounts.Models.Account;

namespace Fitz.Features.Blackjack.Modals
{
    [Table("blackjack_games")]
    public class BlackjackGame
    {
        public BlackjackGame()
        {
            Players = new List<BlackjackPlayers>();
            Deck = new Deck();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("jackpot")]
        public int Jackpot { get; set; }

        [Column("message_id")]
        public ulong MessageId { get; set; }

        [Column("status")]
        public BlackjackGameStatus Status { get; set; }

        [Column("type")]
        public GameType Type { get; set; }

        [Column("started")]
        public DateTime Started { get; set; }

        [Column("updated")]
        public DateTime Updated { get; set; }

        [Column("ended")]
        public DateTime? Ended { get; set; }

        [NotMapped]
        public List<BlackjackPlayers> Players { get; set; }

        [NotMapped]
        public AccountModel Dealer { get; set; }

        [Column("deck_json")]
        public string? DeckJson { get; set; }

        [NotMapped]
        public Deck Deck { get; set; }

        public void InitializeDealer(AccountModel dealer)
        {
            Dealer = dealer;
            Players.Add(new BlackjackPlayers
            {
                GameId = Id,
                UserId = dealer.Id,
                Bet = 0,
                HasTurn = false,
                IsDealer = true,
                IsWinner = false,
                IsBusted = false,
                Account = dealer,
                Hand = new Hand(true)
            });
        }
    }
}