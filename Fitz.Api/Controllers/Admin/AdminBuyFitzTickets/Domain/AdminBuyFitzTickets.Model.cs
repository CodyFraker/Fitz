namespace Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Domain;

public record AdminBuyFitzTicketsModel(
    string Message)
{
    public static AdminBuyFitzTicketsModel From(string message)
    {
        return new AdminBuyFitzTicketsModel(Message: message);
    }
}
