using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Database.Entities
{
    [Table("poll_options")]
    public class PollOptionsEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("poll_id")]
        public int PollId { get; set; }

        [Column("answer")]
        public string Answer { get; set; }

        [Column("emoji_name")]
        public string EmojiName { get; set; }

        [Column("emoji_id")]
        public ulong? EmojiId { get; set; }
    }
}
