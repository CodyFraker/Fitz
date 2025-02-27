using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Lottery.Models
{
    [Table("Lotteries")]
    public class Lottery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Start date of the lottery drawing.
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the lottery drawing.
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Total amount of beer in the pool. This will be how much someone will win with some exceptions.
        /// </summary>
        [Required]
        public int Pool { get; set; }

        [Required]
        public bool CurrentLottery { get; set; }

        public int? WinningTicketId { get; set; }

        [ForeignKey("WinningTicketId")]
        public virtual Ticket WinningTicket { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}