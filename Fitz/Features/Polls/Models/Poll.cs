using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Polls.Models
{
    /// <summary>
    /// Represents a poll in the system
    /// </summary>
    [Table("polls")]
    public class Poll
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The Discord user ID of the poll creator
        /// </summary>
        [Column("account_id")]
        public ulong AccountId { get; set; }

        /// <summary>
        /// The Discord channel ID where the poll is posted
        /// </summary>
        [Column("channel_id")]
        public ulong ChannelId { get; set; }

        /// <summary>
        /// The Discord message ID of the poll
        /// </summary>
        [Column("message_id")]
        public ulong MessageId { get; set; }

        /// <summary>
        /// The title of the poll
        /// </summary>
        [Column("title")]
        public string Title { get; set; }

        /// <summary>
        /// The description or question of the poll
        /// </summary>
        [Column("description")]
        public string Description { get; set; }

        /// <summary>
        /// The type of poll
        /// </summary>
        [Column("type")]
        public PollType Type { get; set; }

        /// <summary>
        /// The current status of the poll
        /// </summary>
        [Column("status")]
        public PollStatus Status { get; set; }

        /// <summary>
        /// Whether the poll allows multiple votes from the same user
        /// </summary>
        [Column("allow_multiple_votes")]
        public bool AllowMultipleVotes { get; set; }

        /// <summary>
        /// When the poll was created
        /// </summary>
        [Column("created_on")]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// When the poll was evaluated (approved/declined)
        /// </summary>
        [Column("evaluated_on")]
        public DateTime? EvaluatedOn { get; set; }

        /// <summary>
        /// When the poll ends
        /// </summary>
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The options for this poll
        /// </summary>
        [NotMapped]
        public PollOptions Options { get; set; }

        /// <summary>
        /// The votes cast on this poll
        /// </summary>
        [NotMapped]
        public List<Vote> UserVotes { get; set; } = new List<Vote>();
    }
}