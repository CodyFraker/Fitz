using System.ComponentModel.DataAnnotations;
using Fitz.Database.Entities;

namespace Fitz.Api.Models.Requests
{
    public class EvaluatePollRequest
    {
        [Required]
        public PollStatus Status { get; set; }
    }
}
