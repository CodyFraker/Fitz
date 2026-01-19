namespace Fitz.Api.Controllers.Polls.Exceptions;

public class PollAlreadyPostedException : Exception
{
    public PollAlreadyPostedException() : base("Poll has already been posted to Discord")
    {
    }
}
