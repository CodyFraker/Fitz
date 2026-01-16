using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class DeductBeerRequest
    {
        [Required]
        public ulong UserId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }
        
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}
