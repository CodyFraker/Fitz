using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain;

public interface IGetLotteryHistory
{
    Task<List<LotteryEntity>> FindHistoryAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalTicketsAsync(int lotteryId, CancellationToken cancellationToken = default);
    Task<int> GetTotalParticipantsAsync(int lotteryId, CancellationToken cancellationToken = default);
}
