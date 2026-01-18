using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Database.Entities
{
    [Table("winners")]
    public class Winners
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("drawing_id")]
        public int Drawing { get; set; }

        [Column("winning_ticket")]
        public int WinningTicketId { get; set; }

        [Column("payout")]
        public int Payout { get; set; }

        [Column("account_id")]
        public ulong AccountId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
