using Fitz.Api.Controllers.Polls.CreatePoll.Domain;
using Fitz.Database.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Polls.CreatePoll.Http;

[DisplayName("CreatePollRequest")]
public record CreatePollRequestDto
{
    [Required]
    public required ulong AccountId { get; set; }

    [Required]
    public required ulong MessageId { get; set; }

    [Required]
    [MaxLength(128)]
    public required string Question { get; set; }

    [Required]
    public required PollTypeEnum Type { get; set; }

    [Required]
    [MinLength(1)]
    public required List<PollOptionRequestDto> Options { get; set; }

    internal CreatePollCommand ToCommand()
    {
        return CreatePollCommand.From(this);
    }
}

public record PollOptionRequestDto
{
    [Required]
    public required string Answer { get; set; }

    [Required]
    public required string EmojiName { get; set; }

    public ulong? EmojiId { get; set; }
}
