using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class AddVoteRequest
    {
        [Required]
        public ulong UserId { get; set; }

        [Required]
        public int OptionId { get; set; }
    }
}
