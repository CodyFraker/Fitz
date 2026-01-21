using Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Http;

namespace Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Domain;

public record AdminExtendLotteryEndDateCommand(DateTime EndDate)
{
    public static AdminExtendLotteryEndDateCommand From(AdminExtendLotteryEndDateRequestDto request)
    {
        return new AdminExtendLotteryEndDateCommand(EndDate: request.EndDate);
    }
}
