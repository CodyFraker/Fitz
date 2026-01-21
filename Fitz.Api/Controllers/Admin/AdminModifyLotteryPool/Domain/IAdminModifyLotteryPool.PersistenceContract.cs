using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminModifyLotteryPool.Domain;

public interface IAdminModifyLotteryPool
{
    Task<LotteryEntity?> GetCurrentLotteryAsync(CancellationToken cancellationToken = default);
}
