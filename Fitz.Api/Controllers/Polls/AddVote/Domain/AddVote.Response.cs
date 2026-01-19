namespace Fitz.Api.Controllers.Polls.AddVote.Domain;

public record AddVoteResponse
{
    public static AddVoteResponse From(AddVoteModel model)
    {
        return new AddVoteResponse();
    }
}
