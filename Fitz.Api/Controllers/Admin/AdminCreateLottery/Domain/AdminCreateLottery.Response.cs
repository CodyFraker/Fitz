namespace Fitz.Api.Controllers.Admin.AdminCreateLottery.Domain;

public record AdminCreateLotteryResponse(
    string Message)
{
    public static AdminCreateLotteryResponse From(AdminCreateLotteryModel model)
    {
        return new AdminCreateLotteryResponse(Message: model.Message);
    }
}
