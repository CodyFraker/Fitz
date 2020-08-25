namespace Fitz.DB.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("beer")]
    public class Beers
    {
        [Column("id")]
        [Key]
        public ulong Id { get; set; }

        [Column("donated_by")]
        public ulong UserId { get; set; }

        [Column("from_guild")]
        public ulong GuildID { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
