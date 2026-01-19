using Fitz.Database.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Polls.EvaluatePoll.Http;

[DisplayName("EvaluatePollRequest")]
public record EvaluatePollRequestDto
{
    [Required]
    public required PollStatusEnum Status { get; set; }
}
