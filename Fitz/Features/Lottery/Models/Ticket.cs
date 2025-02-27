using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Lottery.Models
{
    [Table("LotteryTickets")]
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int LotteryId { get; set; }

        [ForeignKey("LotteryId")]
        public virtual Lottery Lottery { get; set; }

        [Required]
        public ulong UserId { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Required]
        public int TicketNumber { get; set; }

        public bool IsWinner { get; set; }
    }
}