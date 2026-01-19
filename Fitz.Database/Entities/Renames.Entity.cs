using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Database.Entities
{
    [Table("renames")]
    public class RenamesEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("old_name")]
        public string OldName { get; set; }

        [Column("new_name")]
        public string NewName { get; set; }

        [Column("affected_user_id")]
        public ulong AffectedUserId { get; set; }

        [Column("requested_user_id")]
        public ulong RequestedUserId { get; set; }

        [Column("days")]
        public int? Days { get; set; }

        [Column("cost")]
        public int Cost { get; set; }

        [Column("notified")]
        public bool Notified { get; set; }

        [Column("status")]
        public RenameStatusEnum Status { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("expires")]
        public DateTime? Expiration { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
