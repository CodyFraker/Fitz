using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class SetSafeBalanceRequest
    {
        [Required]
        public ulong UserId { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int SafeBalance { get; set; }
    }
}
