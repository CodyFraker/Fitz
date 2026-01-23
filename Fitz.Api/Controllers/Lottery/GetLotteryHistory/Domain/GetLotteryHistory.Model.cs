using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain;

public record LotteryWinnerModel(
    ulong AccountId,
    string? Username,
    int Payout);

public record LotteryHistoryItemModel(
    int Id,
    DateTime StartDate,
    DateTime EndDate,
    int? Pool,
    int? WinningTicket,
    int TotalTickets,
    int TotalParticipants,
    List<LotteryWinnerModel> Winners);

public record GetLotteryHistoryModel(
    List<LotteryHistoryItemModel> Lotteries,
    int TotalCount,
    int Skip,
    int Take)
{
    public static GetLotteryHistoryModel From(List<LotteryHistoryItemModel> lotteries, int totalCount, int skip, int take)
    {
        return new GetLotteryHistoryModel(
            Lotteries: lotteries,
            TotalCount: totalCount,
            Skip: skip,
            Take: take
        );
    }
}
