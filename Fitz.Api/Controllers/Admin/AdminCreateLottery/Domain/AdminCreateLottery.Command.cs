using Fitz.Api.Controllers.Admin.AdminCreateLottery.Http;

namespace Fitz.Api.Controllers.Admin.AdminCreateLottery.Domain;

public record AdminCreateLotteryCommand(DateTime? StartDate, DateTime? EndDate, int Pool)
{
    public static AdminCreateLotteryCommand From(AdminCreateLotteryRequestDto request)
    {
        return new AdminCreateLotteryCommand(
            StartDate: request.StartDate,
            EndDate: request.EndDate,
            Pool: request.Pool
        );
    }
}
