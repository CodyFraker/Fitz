using Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Http;

namespace Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Domain;

public record AdminBulkUpdateFavorabilityCommand(ulong[] UserIds, int Favorability)
{
    public static AdminBulkUpdateFavorabilityCommand From(BulkUpdateFavorabilityRequestDto request)
    {
        return new AdminBulkUpdateFavorabilityCommand(UserIds: request.UserIds, Favorability: request.Favorability);
    }
}
