namespace Fitz.Api.Controllers.Admin.AdminModifyLotteryPool.Domain;

public record AdminModifyLotteryPoolResponse(
    string Message)
{
    public static AdminModifyLotteryPoolResponse From(AdminModifyLotteryPoolModel model)
    {
        return new AdminModifyLotteryPoolResponse(Message: model.Message);
    }
}
