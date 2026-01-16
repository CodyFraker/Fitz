using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class SetTicketAmountRequest
    {
        [Required]
        public ulong UserId { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int Amount { get; set; }
    }
}
