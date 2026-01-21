using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminCancelLottery.Domain;

public interface IAdminCancelLottery
{
    Task<LotteryEntity?> GetCurrentLotteryAsync(CancellationToken cancellationToken = default);
}
