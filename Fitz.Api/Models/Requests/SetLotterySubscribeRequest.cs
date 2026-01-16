using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class SetLotterySubscribeRequest
    {
        [Required]
        public ulong UserId { get; set; }
        
        [Required]
        public bool Subscribe { get; set; }
    }
}
