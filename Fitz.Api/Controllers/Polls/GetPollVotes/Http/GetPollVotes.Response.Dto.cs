using Fitz.Api.Controllers.Polls.GetPollVotes.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Polls.GetPollVotes.Http;

[DisplayName("GetPollVotesResponse")]
public record GetPollVotesResponseDto
{
    [Required]
    public required List<VoteResponseItemDto> Votes { get; set; }

    public static GetPollVotesResponseDto From(GetPollVotesResponse response)
    {
        return new GetPollVotesResponseDto
        {
            Votes = response.Votes.Select(v => new VoteResponseItemDto
            {
                Id = v.Id,
                PollId = v.PollId,
                Choice = v.Choice,
                UserId = v.UserId,
                Timestamp = v.Timestamp
            }).ToList()
        };
    }
}

public record VoteResponseItemDto
{
    [Required]
    public required int Id { get; set; }

    [Required]
    public required int PollId { get; set; }

    public int? Choice { get; set; }

    [Required]
    public required ulong UserId { get; set; }

    [Required]
    public required DateTime Timestamp { get; set; }
}
