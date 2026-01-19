using System.ComponentModel.DataAnnotations;
using Fitz.Database.Entities;

namespace Fitz.Api.Models.Requests
{
    public class EvaluatePollRequest
    {
        [Required]
        public PollStatusEnum Status { get; set; }
    }
}
