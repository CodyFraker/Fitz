using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Accounts.Models
{
    [Table("accounts")]
    public class Account
    {
        [Key]
        [Column("id")]
        public ulong Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("beer")]
        public int Beer { get; set; }

        [Column("lifetime_beer")]
        public int LifetimeBeer { get; set; }

        [Column("favorability")]
        public int Favorability { get; set; }

        [Column("renames")]
        public int Renames { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }
    }
}