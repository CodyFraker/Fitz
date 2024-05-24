using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Models
{
    [Table("winners")]
    public class Winners
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("drawing_id")]
        public int Drawing { get; set; }

        [Column("winning_ticket")]
        public int WinningTicketId { get; set; }

        [Column("payout")]
        public int Payout { get; set; }

        [Column("account_id")]
        public ulong AccountId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}