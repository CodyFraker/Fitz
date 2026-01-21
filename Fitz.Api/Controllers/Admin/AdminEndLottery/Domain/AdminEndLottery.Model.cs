namespace Fitz.Api.Controllers.Admin.AdminEndLottery.Domain;

public record AdminEndLotteryModel(
    string Message)
{
    public static AdminEndLotteryModel From(string message)
    {
        return new AdminEndLotteryModel(Message: message);
    }
}
