using System.ComponentModel.DataAnnotations;
using Fitz.Features.Polls.Models;

namespace Fitz.Api.Models.Requests
{
    public class EvaluatePollRequest
    {
        [Required]
        public PollStatus Status { get; set; }
    }
}
