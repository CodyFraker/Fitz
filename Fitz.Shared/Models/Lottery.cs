using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Shared.Models
{
    [Table("lotteries")]
    public class Lottery
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Column("prize_pool")]
        public int PrizePool { get; set; }
        
        [Column("start_date")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        [Column("draw_date")]
        public DateTime DrawDate { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        
        [Column("winner_id")]
        public ulong? WinnerId { get; set; }
        
        [NotMapped]
        public virtual ICollection<LotteryEntry> Entries { get; set; } = new List<LotteryEntry>();
    }
} 