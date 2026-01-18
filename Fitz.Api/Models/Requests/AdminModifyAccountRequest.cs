using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class AdminModifyAccountRequest
    {
        [Required]
        public ulong UserId { get; set; }
        
        public int? Beer { get; set; }
        
        public int? LifetimeBeer { get; set; }
        
        public int? SafeBalance { get; set; }
        
        public int? Favorability { get; set; }
        
        public bool? SubscribeToLottery { get; set; }
        
        public int? SubscribeTickets { get; set; }
        
        public bool? Deactivated { get; set; }
    }
}
