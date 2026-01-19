namespace Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain;

public record GetLotteryHistoryResponse(
    List<LotteryHistoryItemModel> Lotteries,
    int TotalCount,
    int Skip,
    int Take)
{
    public static GetLotteryHistoryResponse From(GetLotteryHistoryModel model)
    {
        return new GetLotteryHistoryResponse(
            Lotteries: model.Lotteries,
            TotalCount: model.TotalCount,
            Skip: model.Skip,
            Take: model.Take
        );
    }
}
