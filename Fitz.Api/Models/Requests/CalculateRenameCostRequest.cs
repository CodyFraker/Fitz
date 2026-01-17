using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class CalculateRenameCostRequest
    {
        [Required]
        public ulong AffectedUserId { get; set; }

        [Required]
        public ulong RequestedUserId { get; set; }

        [Required]
        [Range(1, 365)]
        public double Days { get; set; }

        [Required]
        [MaxLength(32)]
        public string NewName { get; set; } = string.Empty;
    }
}
