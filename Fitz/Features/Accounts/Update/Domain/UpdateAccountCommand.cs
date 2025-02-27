using System;

namespace Fitz.Features.Accounts.Update.Domain
{
    public class UpdateAccountCommand
    {
        public ulong Id { get; set; }
        public string? Username { get; set; }
        public int? SafeBalance { get; set; }
        public bool? SubscribeToLottery { get; set; }
        public int? SubscribeTickets { get; set; }
        public int? Favorability { get; set; }
        public bool? Deactivated { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
    }
} 