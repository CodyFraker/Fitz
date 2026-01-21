namespace Fitz.Api.Controllers.Admin.AdminEndLottery.Domain;

public record AdminEndLotteryResponse(
    string Message)
{
    public static AdminEndLotteryResponse From(AdminEndLotteryModel model)
    {
        return new AdminEndLotteryResponse(Message: model.Message);
    }
}
