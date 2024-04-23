using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Models
{
    [Table("tickets")]
    public class Ticket
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("drawing")]
        public int Drawing { get; set; }

        [Column("number")]
        public int Number { get; set; }

        [Column("user")]
        public ulong User { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}