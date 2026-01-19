namespace Fitz.Api.Controllers.Polls.Exceptions;

public class PollNotFound : Exception
{
    public PollNotFound() : base("Poll not found")
    {
    }

    public PollNotFound(int pollId) : base($"Poll with ID {pollId} not found")
    {
    }

    public PollNotFound(ulong messageId) : base($"Poll with message ID {messageId} not found")
    {
    }
}
