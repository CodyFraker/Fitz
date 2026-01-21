namespace Fitz.Api.Controllers.Admin.AdminDeletePoll.Domain;

public record AdminDeletePollResponse(
    string Message)
{
    public static AdminDeletePollResponse From(AdminDeletePollModel model)
    {
        return new AdminDeletePollResponse(Message: model.Message);
    }
}
