using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class AdminExtendLotteryEndDateRequest
    {
        [Required]
        public DateTime EndDate { get; set; }
    }
}
