using Fitz.Core.Commands;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Polls.Models
{
    [Table("poll_options")]
    public class PollOptions
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("poll_id")]
        public int PollId { get; set; }

        [Column("answer")]
        public string Answer { get; set; }

        /// <summary>
        /// The Discord name for the emoji.
        /// </summary>
        [Column("emoji_name")]
        public string EmojiName { get; set; }

        /// <summary>
        /// The Discord ID for the emoji.
        /// If emoji is a default emoji, this will be 0.
        /// </summary>
        [Column("emoji_id")]
        public ulong? EmojiId { get; set; }
    }
}