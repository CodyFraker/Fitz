namespace Fitz.Api.Controllers.Admin.AdminCancelLottery.Domain;

public record AdminCancelLotteryCommand
{
    public static AdminCancelLotteryCommand From()
    {
        return new AdminCancelLotteryCommand();
    }
}
