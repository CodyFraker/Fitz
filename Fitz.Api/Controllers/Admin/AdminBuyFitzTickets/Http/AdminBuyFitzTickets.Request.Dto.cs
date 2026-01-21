using Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Http;

[DisplayName("AdminBuyFitzTicketsRequest")]
public record AdminBuyFitzTicketsRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public required int Tickets { get; set; }

    internal AdminBuyFitzTicketsCommand ToCommand()
    {
        return AdminBuyFitzTicketsCommand.From(this);
    }
}
