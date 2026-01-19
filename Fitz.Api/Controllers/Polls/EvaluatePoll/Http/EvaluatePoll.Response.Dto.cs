using Fitz.Api.Controllers.Polls.EvaluatePoll.Domain;
using Fitz.Database.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Polls.EvaluatePoll.Http;

[DisplayName("EvaluatePollResponse")]
public record EvaluatePollResponseDto
{
    [Required]
    public required int Id { get; set; }

    [Required]
    public required ulong AccountId { get; set; }

    [Required]
    public required ulong MessageId { get; set; }

    [Required]
    public required string Question { get; set; }

    [Required]
    public required PollTypeEnum Type { get; set; }

    [Required]
    public required PollStatusEnum Status { get; set; }

    public DateTime? EvaluatedOn { get; set; }

    [Required]
    public required DateTime SubmittedOn { get; set; }

    public static EvaluatePollResponseDto From(EvaluatePollResponse response)
    {
        return new EvaluatePollResponseDto
        {
            Id = response.Id,
            AccountId = response.AccountId,
            MessageId = response.MessageId,
            Question = response.Question,
            Type = response.Type,
            Status = response.Status,
            EvaluatedOn = response.EvaluatedOn,
            SubmittedOn = response.SubmittedOn
        };
    }
}
