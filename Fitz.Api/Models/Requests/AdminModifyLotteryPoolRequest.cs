using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class AdminModifyLotteryPoolRequest
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int Pool { get; set; }
    }
}
