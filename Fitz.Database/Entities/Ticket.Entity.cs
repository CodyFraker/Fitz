using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Database.Entities
{
    [Table("tickets")]
    public class TicketEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("drawing")]
        public int Drawing { get; set; }

        [Column("number")]
        public int Number { get; set; }

        [Column("account_id")]
        public ulong AccountId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
