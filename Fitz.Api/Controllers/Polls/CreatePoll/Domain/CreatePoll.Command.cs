using Fitz.Api.Controllers.Polls.CreatePoll.Http;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.CreatePoll.Domain;

public record CreatePollCommand(
    ulong AccountId,
    ulong MessageId,
    string Question,
    PollTypeEnum Type,
    List<PollOptionCommand> Options)
{
    public static CreatePollCommand From(CreatePollRequestDto request)
    {
        return new CreatePollCommand(
            AccountId: request.AccountId,
            MessageId: request.MessageId,
            Question: request.Question,
            Type: request.Type,
            Options: request.Options.Select(o => new PollOptionCommand(
                Answer: o.Answer,
                EmojiName: o.EmojiName,
                EmojiId: o.EmojiId
            )).ToList()
        );
    }
}

public record PollOptionCommand(
    string Answer,
    string EmojiName,
    ulong? EmojiId);
