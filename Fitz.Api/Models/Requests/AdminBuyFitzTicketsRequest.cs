using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class AdminBuyFitzTicketsRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Tickets { get; set; }
    }
}
