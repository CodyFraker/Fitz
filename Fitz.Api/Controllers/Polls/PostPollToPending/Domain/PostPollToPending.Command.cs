namespace Fitz.Api.Controllers.Polls.PostPollToPending.Domain;

public record PostPollToPendingCommand(int PollId)
{
    public static PostPollToPendingCommand From(int pollId)
    {
        return new PostPollToPendingCommand(PollId: pollId);
    }
}
