using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Polls.AddVote.Http;

[DisplayName("AddVoteRequest")]
public record AddVoteRequestDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    public required int OptionId { get; set; }
}
