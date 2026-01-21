namespace Fitz.Api.Controllers.Admin.AdminCancelLottery.Domain;

public record AdminCancelLotteryResponse(
    string Message)
{
    public static AdminCancelLotteryResponse From(AdminCancelLotteryModel model)
    {
        return new AdminCancelLotteryResponse(Message: model.Message);
    }
}
