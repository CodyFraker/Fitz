using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Bank.Models
{
    [Table("Transactions")]
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public ulong SenderId { get; set; }

        [Required]
        public ulong RecipientId { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(255)]
        public string Reason { get; set; }

        public TransactionType Type { get; set; }
    }

    public enum TransactionType
    {
        Transfer = 0,
        Reward = 1,
        Purchase = 2,
        Refund = 3,
        System = 4
    }
} 