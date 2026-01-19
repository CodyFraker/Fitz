using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Polls.UpdateVote.Http;

[DisplayName("UpdateVoteRequest")]
public record UpdateVoteRequestDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    public required int OptionId { get; set; }
}
