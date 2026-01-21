namespace Fitz.Api.Controllers.Admin.AdminDeletePoll.Domain;

public record AdminDeletePollCommand(int Id)
{
    public static AdminDeletePollCommand From(int id)
    {
        return new AdminDeletePollCommand(Id: id);
    }
}
