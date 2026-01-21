using Fitz.Api.Controllers.Admin.AdminModifyLotteryPool.Http;

namespace Fitz.Api.Controllers.Admin.AdminModifyLotteryPool.Domain;

public record AdminModifyLotteryPoolCommand(int Pool)
{
    public static AdminModifyLotteryPoolCommand From(AdminModifyLotteryPoolRequestDto request)
    {
        return new AdminModifyLotteryPoolCommand(Pool: request.Pool);
    }
}
