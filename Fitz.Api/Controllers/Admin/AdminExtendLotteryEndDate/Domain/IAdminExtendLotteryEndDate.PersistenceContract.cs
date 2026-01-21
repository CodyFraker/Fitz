using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Domain;

public interface IAdminExtendLotteryEndDate
{
    Task<LotteryEntity?> GetCurrentLotteryAsync(CancellationToken cancellationToken = default);
}
