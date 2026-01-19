namespace Fitz.Api.Controllers.Polls.GetPollOptions.Domain;

public record GetPollOptionsCommand(int PollId)
{
    public static GetPollOptionsCommand From(int pollId)
    {
        return new GetPollOptionsCommand(PollId: pollId);
    }
}
