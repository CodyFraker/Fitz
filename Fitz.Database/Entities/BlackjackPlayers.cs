using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Database.Entities
{
    [Table("blackjack_players")]
    public sealed class BlackjackPlayers
    {
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
        public Account Account { get; set; }

        [Column("cards_json")]
        public string? CardsJson { get; set; }

        [NotMapped]
        public object Hand { get; set; }
    }
}
