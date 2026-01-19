using Fitz.Api.Controllers.Polls.GetPollOptions.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Polls.GetPollOptions.Http;

[DisplayName("GetPollOptionsResponse")]
public record GetPollOptionsResponseDto
{
    [Required]
    public required List<PollOptionResponseItemDto> Options { get; set; }

    public static GetPollOptionsResponseDto From(GetPollOptionsResponse response)
    {
        return new GetPollOptionsResponseDto
        {
            Options = response.Options.Select(o => new PollOptionResponseItemDto
            {
                Id = o.Id,
                PollId = o.PollId,
                Answer = o.Answer,
                EmojiName = o.EmojiName,
                EmojiId = o.EmojiId
            }).ToList()
        };
    }
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
