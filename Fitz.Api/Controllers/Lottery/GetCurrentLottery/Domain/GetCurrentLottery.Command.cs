namespace Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain;

public record GetCurrentLotteryCommand
{
    public static GetCurrentLotteryCommand From()
    {
        return new GetCurrentLotteryCommand();
    }
}
