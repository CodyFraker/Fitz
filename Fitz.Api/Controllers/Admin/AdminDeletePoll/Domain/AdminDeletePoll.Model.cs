namespace Fitz.Api.Controllers.Admin.AdminDeletePoll.Domain;

public record AdminDeletePollModel(
    string Message)
{
    public static AdminDeletePollModel From(string message)
    {
        return new AdminDeletePollModel(Message: message);
    }
}
