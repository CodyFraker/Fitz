using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Polls.Models
{
    [Table("polls")]
    public class Poll
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("message_id")]
        public ulong MessageId { get; set; }

        [Column("question")]
        public string Question { get; set; }

        [Column("type")]
        public PollType Type { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}