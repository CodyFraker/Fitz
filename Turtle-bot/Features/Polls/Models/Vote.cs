using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Polls.Models
{
    [Table("votes")]
    public class Vote
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("poll_id")]
        public int PollId { get; set; }

        [Column("poll_option_id")]
        public int? Choice { get; set; }

        [Column("user_id")]
        public ulong UserId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}