using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;

public interface ICalculateRenameCost
{
    Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default);
    Task<SettingsEntity?> GetSettingsAsync(CancellationToken cancellationToken = default);
}
