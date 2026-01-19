using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollVotes.Domain;

public record GetPollVotesResponse(
    List<VoteResponseItem> Votes)
{
    public static GetPollVotesResponse From(GetPollVotesModel model)
    {
        return new GetPollVotesResponse(
            Votes: model.Votes.Select(v => new VoteResponseItem(
                Id: v.Id,
                PollId: v.PollId,
                Choice: v.Choice,
                UserId: v.UserId,
                Timestamp: v.Timestamp
            )).ToList()
        );
    }
}

public record VoteResponseItem(
    int Id,
    int PollId,
    int? Choice,
    ulong UserId,
    DateTime Timestamp);
