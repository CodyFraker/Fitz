using Fitz.Features.Rename.Models;

namespace Fitz.Api.Models.Responses
{
    public class RenameResponse
    {
        public int Id { get; set; }
        public string? OldName { get; set; }
        public string NewName { get; set; } = string.Empty;
        public ulong AffectedUserId { get; set; }
        public ulong RequestedUserId { get; set; }
        public int? Days { get; set; }
        public int Cost { get; set; }
        public bool Notified { get; set; }
        public RenameStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Expiration { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
