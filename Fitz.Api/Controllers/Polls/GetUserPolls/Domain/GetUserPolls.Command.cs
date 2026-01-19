namespace Fitz.Api.Controllers.Polls.GetUserPolls.Domain;

public record GetUserPollsCommand(ulong UserId)
{
    public static GetUserPollsCommand From(ulong userId)
    {
        return new GetUserPollsCommand(UserId: userId);
    }
}
