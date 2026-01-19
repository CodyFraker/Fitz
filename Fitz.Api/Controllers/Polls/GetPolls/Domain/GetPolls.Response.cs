using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.GetPolls.Domain;

public record GetPollsResponse(
    List<PollResponseItem> Polls)
{
    public static GetPollsResponse From(GetPollsModel model)
    {
        return new GetPollsResponse(
            Polls: model.Polls.Select(p => new PollResponseItem(
                Id: p.Id,
                AccountId: p.AccountId,
                MessageId: p.MessageId,
                Question: p.Question,
                Type: p.Type,
                Status: p.Status,
                EvaluatedOn: p.EvaluatedOn,
                SubmittedOn: p.SubmittedOn
            )).ToList()
        );
    }
}

public record PollResponseItem(
    int Id,
    ulong AccountId,
    ulong MessageId,
    string Question,
    PollTypeEnum Type,
    PollStatusEnum Status,
    DateTime? EvaluatedOn,
    DateTime SubmittedOn);
