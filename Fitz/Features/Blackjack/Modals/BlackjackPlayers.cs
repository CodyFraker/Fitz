using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AccountModel = Fitz.Features.Accounts.Models.Account;

namespace Fitz.Features.Blackjack.Modals
{
    [Table("blackjack_players")]
    public sealed class BlackjackPlayers
    {
        public BlackjackPlayers()
        {
            Hand = new Hand();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("game_id")]
        public int GameId { get; set; }

        [Column("account_id")]
        public ulong UserId { get; set; }

        [Column("bet")]
        public int Bet { get; set; }

        [Column("hasTurn")]
        public bool HasTurn { get; set; }

        [Column("isDealer")]
        public bool IsDealer { get; set; }

        [Column("isWinner")]
        public bool IsWinner { get; set; }

        [Column("isBusted")]
        public bool IsBusted { get; set; }

        [NotMapped]
        public AccountModel Account { get; set; }

        [Column("cards_json")]
        public string? CardsJson 
        { 
            get => Hand != null ? JsonSerializer.Serialize(Hand.Cards) : null;
            set
            {
                if (value != null)
                {
                    Hand = new Hand(IsDealer);
                    var cards = JsonSerializer.Deserialize<List<Card>>(value);
                    foreach (var card in cards)
                    {
                        Hand.AddCard(card);
                    }
                }
            }
        }

        [NotMapped]
        public Hand Hand { get; set; }
    }
}