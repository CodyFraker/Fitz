using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Bank.Models
{
    [Table("trasnactions")]
    public class Transaction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("sender")]
        public ulong Sender { get; set; }

        [Column("recipient")]
        public ulong Recipient { get; set; }

        [Column("amount")]
        public int Amount { get; set; }

        [Column("reason")]
        public Reason Reason { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}