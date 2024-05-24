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

        [Column("poll")]
        public int PollId { get; set; }

        [Column("emoji_name")]
        public string Name { get; set; }

        [Column("emoji_id")]
        public ulong? EmojiId { get; set; }
    }
}