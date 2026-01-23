using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain;

public interface IGetCurrentLottery
{
    Task<LotteryEntity?> FindCurrentAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalTicketsAsync(int lotteryId, CancellationToken cancellationToken = default);
    Task<int> GetTotalParticipantsAsync(int lotteryId, CancellationToken cancellationToken = default);
    Task<int> GetLastWinningTicketAsync(CancellationToken cancellationToken = default);
}
