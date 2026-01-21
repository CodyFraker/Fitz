using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Domain;

public interface IAdminBuyFitzTickets
{
    Task<LotteryEntity?> GetCurrentLotteryAsync(CancellationToken cancellationToken = default);
}
