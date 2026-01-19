using Fitz.Api.Controllers.Polls.UpdateVote.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Polls.UpdateVote.Http;

[DisplayName("UpdateVoteResponse")]
public record UpdateVoteResponseDto
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

    public static UpdateVoteResponseDto From(UpdateVoteResponse response)
    {
        return new UpdateVoteResponseDto
        {
            Id = response.Id,
            PollId = response.PollId,
            Choice = response.Choice,
            UserId = response.UserId,
            Timestamp = response.Timestamp
        };
    }
}
