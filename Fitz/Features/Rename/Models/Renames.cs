using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.Rename.Models
{
    [Table("renames")]
    public class Renames
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// The affected user's old name.
        /// </summary>
        [Column("old_name")]
        public string OldName { get; set; }

        /// <summary>
        /// The affected user's new name.
        /// </summary>
        [Column("new_name")]
        public string NewName { get; set; }

        /// <summary>
        /// User whose name has been requested to be changed.
        /// </summary>
        [Column("affected_user_id")]
        public ulong AffectedUserId { get; set; }

        /// <summary>
        /// The user who is paying to change a user's name.
        /// </summary>
        [Column("requested_user_id")]
        public ulong RequestedUserId { get; set; }

        /// <summary>
        /// Amount of days the name should be changed for.
        /// </summary>
        [Column("days")]
        public int? Days { get; set; }

        /// <summary>
        /// The total cost of the rename.
        /// </summary>
        [Column("cost")]
        public int Cost { get; set; }

        /// <summary>
        /// Whether the affected user has been notified of the name change.
        /// </summary>
        [Column("notified")]
        public bool Notified { get; set; }

        [Column("status")]
        public RenameStatus Status { get; set; }

        /// <summary>
        /// The date in which this rename should start.
        /// </summary>
        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The date in which this rename should expire.
        /// </summary>
        [Column("expires")]
        public DateTime? Expiration { get; set; }

        /// <summary>
        /// Timestamp in which this rename was created.
        /// </summary>
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}