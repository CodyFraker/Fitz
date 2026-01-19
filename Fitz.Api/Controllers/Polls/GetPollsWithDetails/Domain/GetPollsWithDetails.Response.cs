using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPollsWithDetails.Domain;

public record GetPollsWithDetailsResponse(
    List<PollWithDetailsResponseItem> Polls,
    int TotalCount,
    int Skip,
    int Take)
{
    public static GetPollsWithDetailsResponse From(GetPollsWithDetailsModel model)
    {
        return new GetPollsWithDetailsResponse(
            Polls: model.Polls.Select(p => new PollWithDetailsResponseItem(
                Id: p.Poll.Id,
                AccountId: p.Poll.AccountId,
                MessageId: p.Poll.MessageId,
                Question: p.Poll.Question,
                Type: p.Poll.Type,
                Status: p.Poll.Status,
                EvaluatedOn: p.Poll.EvaluatedOn,
                SubmittedOn: p.Poll.SubmittedOn,
                Options: p.Options.Select(o => new PollOptionResponseItem(
                    Id: o.Id,
                    PollId: o.PollId,
                    Answer: o.Answer,
                    EmojiName: o.EmojiName,
                    EmojiId: o.EmojiId
                )).ToList(),
                TotalVotes: p.TotalVotes,
                OptionVoteCounts: p.OptionVoteCounts
            )).ToList(),
            TotalCount: model.TotalCount,
            Skip: model.Skip,
            Take: model.Take
        );
    }
}

public record PollWithDetailsResponseItem(
    int Id,
    ulong AccountId,
    ulong MessageId,
    string Question,
    PollTypeEnum Type,
    PollStatusEnum Status,
    DateTime? EvaluatedOn,
    DateTime SubmittedOn,
    List<PollOptionResponseItem> Options,
    int TotalVotes,
    Dictionary<int, int> OptionVoteCounts);

public record PollOptionResponseItem(
    int Id,
    int PollId,
    string Answer,
    string EmojiName,
    ulong? EmojiId);
