using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Lottery.BuyTickets.Domain;

public interface IBuyTickets
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<SettingsEntity?> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<LotteryEntity?> GetCurrentLotteryAsync(CancellationToken cancellationToken = default);
    Task<List<TicketEntity>> GetUserTicketsAsync(ulong userId, int lotteryId, CancellationToken cancellationToken = default);
    Task<List<TicketEntity>> CreateTicketsAsync(ulong userId, int lotteryId, int count, CancellationToken cancellationToken = default);
    Task AddToPoolAsync(int lotteryId, int amount, CancellationToken cancellationToken = default);
}
