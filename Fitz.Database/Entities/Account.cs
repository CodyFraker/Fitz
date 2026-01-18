using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Database.Entities
{
    [Table("accounts")]
    public class Account
    {
        [Key]
        [Column("id")]
        public ulong Id { get; set; }

        [Column("username")]
        public string? Username { get; set; }

        [Column("beer")]
        public int Beer { get; set; }

        [Column("lifetime_beer")]
        public int LifetimeBeer { get; set; }

        [Column("safe_balance")]
        public int safeBalance { get; set; }

        [Column("favorability")]
        public int Favorability { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("last_seen")]
        public DateTime LastSeenDate { get; set; }

        [Column("last_active")]
        public DateTime LastActivityDate { get; set; }

        [Column("lottery_subscribe")]
        public bool subscribeToLottery { get; set; }

        [Column("subscribe_tickets")]
        public int SubscribeTickets { get; set; }

        [Column("deactivated")]
        public bool Deactivated { get; set; }
    }
}
