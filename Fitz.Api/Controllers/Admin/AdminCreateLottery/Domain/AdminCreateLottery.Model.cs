namespace Fitz.Api.Controllers.Admin.AdminCreateLottery.Domain;

public record AdminCreateLotteryModel(
    string Message)
{
    public static AdminCreateLotteryModel From(string message)
    {
        return new AdminCreateLotteryModel(Message: message);
    }
}
