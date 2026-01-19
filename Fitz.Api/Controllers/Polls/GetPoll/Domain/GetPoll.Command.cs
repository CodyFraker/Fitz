namespace Fitz.Api.Controllers.Polls.GetPoll.Domain;

public record GetPollCommand(
    int? PollId,
    ulong? MessageId)
{
    public static GetPollCommand FromId(int pollId)
    {
        return new GetPollCommand(PollId: pollId, MessageId: null);
    }

    public static GetPollCommand FromMessageId(ulong messageId)
    {
        return new GetPollCommand(PollId: null, MessageId: messageId);
    }
}
