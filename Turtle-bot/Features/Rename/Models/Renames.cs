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
        /// Timestamp in which this event took place.
        /// </summary>
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}