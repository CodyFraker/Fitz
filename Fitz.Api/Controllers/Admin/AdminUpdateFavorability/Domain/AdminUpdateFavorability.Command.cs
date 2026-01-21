using Fitz.Api.Controllers.Admin.AdminUpdateFavorability.Http;

namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorability.Domain;

public record AdminUpdateFavorabilityCommand(ulong UserId, int Favorability)
{
    public static AdminUpdateFavorabilityCommand From(ulong userId, UpdateFavorabilityRequestDto request)
    {
        return new AdminUpdateFavorabilityCommand(UserId: userId, Favorability: request.Favorability);
    }
}
