using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class TransferBeerRequest
    {
        [Required]
        public ulong SenderId { get; set; }
        
        [Required]
        public ulong RecipientId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Amount { get; set; }
    }
}
