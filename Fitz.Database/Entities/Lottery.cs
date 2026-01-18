using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Database.Entities
{
    [Table("lottery")]
    public class Lottery
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("winning_ticket")]
        public int? WinningTicket { get; set; }

        [Column("pool")]
        public int? Pool { get; set; }

        [Column("current")]
        public bool CurrentLottery { get; set; }
    }
}
