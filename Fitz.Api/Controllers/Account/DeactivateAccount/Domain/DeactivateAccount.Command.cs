using Fitz.Api.Controllers.Account.DeactivateAccount.Http;

namespace Fitz.Api.Controllers.Account.DeactivateAccount.Domain;

public record DeactivateAccountCommand(ulong UserId)
{
    public static DeactivateAccountCommand From(DeactivateAccountRequestDto request)
    {
        return new DeactivateAccountCommand(UserId: request.UserId);
    }
}
