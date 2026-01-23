using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain;

public interface IGetLotteryStatistics
{
    Task<List<LotteryEntity>> FindAllLotteriesAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalTicketsAsync(int lotteryId, CancellationToken cancellationToken = default);
    Task<List<WinnersEntity>> GetAllWinnersAsync(CancellationToken cancellationToken = default);
    Task<int> GetTicketCountForWinnerAsync(int drawingId, ulong accountId, CancellationToken cancellationToken = default);
}
