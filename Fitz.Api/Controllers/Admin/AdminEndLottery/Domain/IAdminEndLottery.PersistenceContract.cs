using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminEndLottery.Domain;

public interface IAdminEndLottery
{
    Task<LotteryEntity?> GetCurrentLotteryAsync(CancellationToken cancellationToken = default);
}
