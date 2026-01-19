using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollOptions.Domain;

public record GetPollOptionsResponse(
    List<PollOptionResponseItem> Options)
{
    public static GetPollOptionsResponse From(GetPollOptionsModel model)
    {
        return new GetPollOptionsResponse(
            Options: model.Options.Select(o => new PollOptionResponseItem(
                Id: o.Id,
                PollId: o.PollId,
                Answer: o.Answer,
                EmojiName: o.EmojiName,
                EmojiId: o.EmojiId
            )).ToList()
        );
    }
}

public record PollOptionResponseItem(
    int Id,
    int PollId,
    string Answer,
    string EmojiName,
    ulong? EmojiId);
