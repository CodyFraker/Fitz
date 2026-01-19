namespace Fitz.Api.Controllers.Polls.Exceptions;

public class PollOptionNotFound : Exception
{
    public PollOptionNotFound() : base("Poll option not found")
    {
    }

    public PollOptionNotFound(int optionId) : base($"Poll option with ID {optionId} not found")
    {
    }
}
