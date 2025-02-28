using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Shared.Models
{
    [Table("transactions")]
    public class Transaction
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Column("sender_id")]
        public ulong SenderId { get; set; }
        
        [Column("recipient_id")]
        public ulong RecipientId { get; set; }
        
        [Column("amount")]
        public int Amount { get; set; }
        
        [Column("reason")]
        public string Reason { get; set; } = string.Empty;
        
        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
} 