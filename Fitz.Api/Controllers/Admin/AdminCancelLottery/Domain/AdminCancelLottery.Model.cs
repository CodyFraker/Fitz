namespace Fitz.Api.Controllers.Admin.AdminCancelLottery.Domain;

public record AdminCancelLotteryModel(
    string Message)
{
    public static AdminCancelLotteryModel From(string message)
    {
        return new AdminCancelLotteryModel(Message: message);
    }
}
