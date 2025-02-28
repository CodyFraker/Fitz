using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Shared.Models
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
        public int SafeBalance { get; set; }
        
        [Column("favorability")]
        public int Favorability { get; set; }
        
        [Column("lottery_subscribe")]
        public bool SubscribeToLottery { get; set; }
        
        [Column("subscribe_tickets")]
        public int SubscribeTickets { get; set; } = 1;
        
        [Column("deactivated")]
        public bool Deactivated { get; set; }
        
        [NotMapped]
        public bool IsActive => !Deactivated;
    }
} 