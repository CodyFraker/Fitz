using System.ComponentModel.DataAnnotations;
using Fitz.Features.Rename.Models;

namespace Fitz.Api.Models.Requests
{
    public class CreateRenameRequest
    {
        [Required]
        [MaxLength(32)]
        public string NewName { get; set; } = string.Empty;

        [Required]
        public ulong AffectedUserId { get; set; }

        [Required]
        public ulong RequestedUserId { get; set; }

        [Required]
        [Range(1, 365)]
        public int Days { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? Expiration { get; set; }

        public RenameStatus? Status { get; set; }
    }
}
