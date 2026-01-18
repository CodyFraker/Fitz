using System.ComponentModel.DataAnnotations;
using Fitz.Database.Entities;

namespace Fitz.Api.Models.Requests
{
    public class CreatePollRequest
    {
        [Required]
        public ulong AccountId { get; set; }

        [Required]
        public ulong MessageId { get; set; }

        [Required]
        [MaxLength(128)]
        public string Question { get; set; } = string.Empty;

        [Required]
        public PollType Type { get; set; }

        [Required]
        [MinLength(1)]
        public List<PollOptionRequest> Options { get; set; } = new();
    }
}
