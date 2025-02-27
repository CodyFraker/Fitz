using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Lottery.Models
{
    [Table("LotteryWinners")]
    public class Winner
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int LotteryId { get; set; }

        [ForeignKey("LotteryId")]
        public virtual Lottery Lottery { get; set; }

        [Required]
        public int TicketId { get; set; }

        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; }

        [Required]
        public ulong UserId { get; set; }

        [Required]
        public int PrizeAmount { get; set; }

        [Required]
        public DateTime WinDate { get; set; }

        public bool PrizeClaimed { get; set; }

        public DateTime? ClaimDate { get; set; }
    }
}