using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class AdminCreateLotteryRequest
    {
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Range(0, int.MaxValue)]
        public int Pool { get; set; }
    }
}
