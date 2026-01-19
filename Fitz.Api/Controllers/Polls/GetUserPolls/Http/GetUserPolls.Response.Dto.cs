using Fitz.Api.Controllers.Polls.GetUserPolls.Domain;
using Fitz.Database.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Polls.GetUserPolls.Http;

[DisplayName("GetUserPollsResponse")]
public record GetUserPollsResponseDto
{
    [Required]
    public required List<PollWithDetailsResponseItemDto> Polls { get; set; }

    public static GetUserPollsResponseDto From(GetUserPollsResponse response)
    {
        return new GetUserPollsResponseDto
        {
            Polls = response.Polls.Select(p => new PollWithDetailsResponseItemDto
            {
                Id = p.Id,
                AccountId = p.AccountId,
                MessageId = p.MessageId,
                Question = p.Question,
                Type = p.Type,
                Status = p.Status,
                EvaluatedOn = p.EvaluatedOn,
                SubmittedOn = p.SubmittedOn,
                Options = p.Options.Select(o => new PollOptionResponseItemDto
                {
                    Id = o.Id,
                    PollId = o.PollId,
                    Answer = o.Answer,
                    EmojiName = o.EmojiName,
                    EmojiId = o.EmojiId
                }).ToList(),
                TotalVotes = p.TotalVotes,
                OptionVoteCounts = p.OptionVoteCounts
            }).ToList()
        };
    }
}

public record PollWithDetailsResponseItemDto
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

    [Required]
    public required List<PollOptionResponseItemDto> Options { get; set; }

    [Required]
    public required int TotalVotes { get; set; }

    [Required]
    public required Dictionary<int, int> OptionVoteCounts { get; set; }
}

public record PollOptionResponseItemDto
{
    [Required]
    public required int Id { get; set; }

    [Required]
    public required int PollId { get; set; }

    [Required]
    public required string Answer { get; set; }

    [Required]
    public required string EmojiName { get; set; }

    public ulong? EmojiId { get; set; }
}
