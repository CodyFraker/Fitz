namespace Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Domain;

public record AdminBuyFitzTicketsResponse(
    string Message)
{
    public static AdminBuyFitzTicketsResponse From(AdminBuyFitzTicketsModel model)
    {
        return new AdminBuyFitzTicketsResponse(Message: model.Message);
    }
}
