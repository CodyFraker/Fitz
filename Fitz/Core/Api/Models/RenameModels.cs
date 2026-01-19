using System;
using Fitz.Database.Entities;

namespace Fitz.Core.Api.Models
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
        public RenameStatusEnum Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Expiration { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class RenameCostResponse
    {
        public int Cost { get; set; }
    }

    public class CreateRenameRequest
    {
        public string NewName { get; set; } = string.Empty;
        public ulong AffectedUserId { get; set; }
        public ulong RequestedUserId { get; set; }
        public int Days { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Expiration { get; set; }
        public RenameStatusEnum? Status { get; set; }
    }

    public class UpdateRenameStatusRequest
    {
        public RenameStatusEnum Status { get; set; }
    }

    public class CalculateRenameCostRequest
    {
        public ulong AffectedUserId { get; set; }
        public ulong RequestedUserId { get; set; }
        public double Days { get; set; }
        public string NewName { get; set; } = string.Empty;
    }
}
