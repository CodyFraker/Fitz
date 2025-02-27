using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Polls.Models
{
    /// <summary>
    /// Represents a vote on a poll
    /// </summary>
    [Table("votes")]
    public class Vote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// The ID of the poll this vote belongs to
        /// </summary>
        [Column("poll_id")]
        public int PollId { get; set; }

        /// <summary>
        /// The Discord user ID of the voter
        /// </summary>
        [Column("user_id")]
        public ulong UserId { get; set; }

        /// <summary>
        /// The ID of the option that was chosen
        /// </summary>
        [Column("poll_option_id")]
        public int Choice { get; set; }

        /// <summary>
        /// When the vote was cast
        /// </summary>
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}