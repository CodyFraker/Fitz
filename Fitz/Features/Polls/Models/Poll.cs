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

        /// <summary>
        /// Id of the account who submitted the poll.
        /// </summary>
        [Column("account_id")]
        public ulong AccountId { get; set; }

        /// <summary>
        /// The Discord Message ID for the pending poll.
        /// </summary>
        [Column("message_id")]
        public ulong MessageId { get; set; }

        /// <summary>
        /// The question the poll is referring to.
        /// </summary>
        [Column("question")]
        public string Question { get; set; }

        /// <summary>
        /// Poll type
        /// </summary>
        [Column("type")]
        public PollType Type { get; set; }

        /// <summary>
        /// True if the poll has been approved.
        /// False if the poll has been declined.
        /// Null if the poll is still in pending state.
        /// </summary>
        [Column("status")]
        public PollStatus Status { get; set; }

        /// <summary>
        /// When the poll was approved or declined.
        /// </summary>
        [Column("evaluated_on")]
        public DateTime? EvaluatedOn { get; set; }

        /// <summary>
        /// When the poll was originally submitted.
        /// </summary>
        [Column("submitted_on")]
        public DateTime SubmittedOn { get; set; }
    }
}