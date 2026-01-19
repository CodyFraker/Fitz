namespace Fitz.Api.Controllers.Polls.AddVote.Domain;

public record AddVoteModel
{
    public static AddVoteModel From()
    {
        return new AddVoteModel();
    }
}
