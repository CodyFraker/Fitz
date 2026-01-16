using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class BuyTicketsRequest
    {
        [Required]
        public ulong UserId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }
    }
}
