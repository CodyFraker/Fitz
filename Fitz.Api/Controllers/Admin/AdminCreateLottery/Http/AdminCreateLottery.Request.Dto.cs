using Fitz.Api.Controllers.Admin.AdminCreateLottery.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Admin.AdminCreateLottery.Http;

[DisplayName("AdminCreateLotteryRequest")]
public record AdminCreateLotteryRequestDto
{
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Range(0, int.MaxValue)]
    public int Pool { get; set; }

    internal AdminCreateLotteryCommand ToCommand()
    {
        return AdminCreateLotteryCommand.From(this);
    }
}
