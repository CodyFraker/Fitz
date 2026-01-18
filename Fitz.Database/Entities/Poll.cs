using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Database.Entities
{
    [Table("polls")]
    public class Poll
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("account_id")]
        public ulong AccountId { get; set; }

        [Column("message_id")]
        public ulong MessageId { get; set; }

        [Column("question")]
        public string Question { get; set; }

        [Column("type")]
        public PollType Type { get; set; }

        [Column("status")]
        public PollStatus Status { get; set; }

        [Column("evaluated_on")]
        public DateTime? EvaluatedOn { get; set; }

        [Column("submitted_on")]
        public DateTime SubmittedOn { get; set; }
    }
}
