namespace Fitz.Api.Controllers.Polls.Exceptions;

public class VoteNotFound : Exception
{
    public VoteNotFound() : base("Vote not found")
    {
    }

    public VoteNotFound(int pollId, ulong userId) : base($"Vote not found for poll {pollId} and user {userId}")
    {
    }
}
