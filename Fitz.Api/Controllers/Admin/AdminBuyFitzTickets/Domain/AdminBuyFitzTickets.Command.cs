using Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Http;

namespace Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Domain;

public record AdminBuyFitzTicketsCommand(int Tickets)
{
    public static AdminBuyFitzTicketsCommand From(AdminBuyFitzTicketsRequestDto request)
    {
        return new AdminBuyFitzTicketsCommand(Tickets: request.Tickets);
    }
}
