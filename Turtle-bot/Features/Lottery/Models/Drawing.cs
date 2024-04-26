using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Lottery.Models
{
    [Table("drawings")]
    public class Drawing
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Start date of the lottery drawing.
        /// </summary>
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the lottery drawing.
        /// </summary>
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The winning ticket ID for this drawing.
        /// </summary>
        [Column("winning_ticket")]
        public int? WinningTicket { get; set; }

        /// <summary>
        /// Total amount of beer in the pool. This will be how much someone will win with some exceptions.
        /// </summary>
        [Column("pool")]
        public int? Pool { get; set; }

        [Column("current")]
        public bool CurrentLottery { get; set; }
    }
}