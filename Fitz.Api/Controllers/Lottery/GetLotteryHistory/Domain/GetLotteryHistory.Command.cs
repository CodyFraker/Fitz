namespace Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain;

public record GetLotteryHistoryCommand(int Skip, int Take)
{
    public static GetLotteryHistoryCommand From(int skip, int take)
    {
        return new GetLotteryHistoryCommand(Skip: skip, Take: take);
    }
}
