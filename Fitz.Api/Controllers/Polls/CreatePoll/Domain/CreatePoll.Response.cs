using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Polls.CreatePoll.Domain;

public record CreatePollResponse(
    int Id,
    ulong AccountId,
    ulong MessageId,
    string Question,
    PollTypeEnum Type,
    PollStatusEnum Status,
    DateTime? EvaluatedOn,
    DateTime SubmittedOn)
{
    public static CreatePollResponse From(CreatePollModel model)
    {
        return new CreatePollResponse(
            Id: model.Poll.Id,
            AccountId: model.Poll.AccountId,
            MessageId: model.Poll.MessageId,
            Question: model.Poll.Question,
            Type: model.Poll.Type,
            Status: model.Poll.Status,
            EvaluatedOn: model.Poll.EvaluatedOn,
            SubmittedOn: model.Poll.SubmittedOn
        );
    }
}
