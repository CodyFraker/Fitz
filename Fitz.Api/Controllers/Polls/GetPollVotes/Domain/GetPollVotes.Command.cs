namespace Fitz.Api.Controllers.Polls.GetPollVotes.Domain;

public record GetPollVotesCommand(int PollId)
{
    public static GetPollVotesCommand From(int pollId)
    {
        return new GetPollVotesCommand(PollId: pollId);
    }
}
