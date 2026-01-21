namespace Fitz.Api.Controllers.Admin.AdminEndLottery.Domain;

public record AdminEndLotteryCommand
{
    public static AdminEndLotteryCommand From()
    {
        return new AdminEndLotteryCommand();
    }
}
