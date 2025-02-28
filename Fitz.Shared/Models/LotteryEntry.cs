using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Shared.Models
{
    [Table("lottery_entries")]
    public class LotteryEntry
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Column("lottery_id")]
        public int LotteryId { get; set; }
        
        [Column("account_id")]
        public ulong AccountId { get; set; }
        
        [Column("entry_date")]
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("LotteryId")]
        public virtual Lottery Lottery { get; set; } = null!;
    }
} 