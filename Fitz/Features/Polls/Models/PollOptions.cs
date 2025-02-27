using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Polls.Models
{
    /// <summary>
    /// Represents the options for a poll
    /// </summary>
    [Table("poll_options")]
    public class PollOptions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// The ID of the poll this option belongs to
        /// </summary>
        [Column("poll_id")]
        public int PollId { get; set; }

        /// <summary>
        /// The name of the emoji used for this option
        /// </summary>
        [Column("emoji_name")]
        public string EmojiName { get; set; }

        /// <summary>
        /// The ID of the emoji used for this option (if it's a custom emoji)
        /// </summary>
        [Column("emoji_id")]
        public ulong? EmojiId { get; set; }

        /// <summary>
        /// The answer text for this option
        /// </summary>
        [Column("answer")]
        public string Answer { get; set; }

        /// <summary>
        /// The list of option values for this poll
        /// </summary>
        [NotMapped]
        public List<string> Values { get; set; } = new List<string>();
    }
}