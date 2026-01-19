using Fitz.Api.Controllers.Polls.GetPolls.Domain;
using Fitz.Database.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Polls.GetPolls.Http;

[DisplayName("GetPollsResponse")]
public record GetPollsResponseDto
{
    [Required]
    public required List<PollResponseItemDto> Polls { get; set; }

    public static GetPollsResponseDto From(GetPollsResponse response)
    {
        return new GetPollsResponseDto
        {
            Polls = response.Polls.Select(p => new PollResponseItemDto
            {
                Id = p.Id,
                AccountId = p.AccountId,
                MessageId = p.MessageId,
                Question = p.Question,
                Type = p.Type,
                Status = p.Status,
                EvaluatedOn = p.EvaluatedOn,
                SubmittedOn = p.SubmittedOn
            }).ToList()
        };
    }
}

public record PollResponseItemDto
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
}
