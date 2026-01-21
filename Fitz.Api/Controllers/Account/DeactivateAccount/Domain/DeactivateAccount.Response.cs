namespace Fitz.Api.Controllers.Account.DeactivateAccount.Domain;

public record DeactivateAccountResponse(
    ulong UserId,
    bool Deactivated)
{
    public static DeactivateAccountResponse From(DeactivateAccountModel model)
    {
        return new DeactivateAccountResponse(
            UserId: model.Account.Id,
            Deactivated: model.Deactivated
        );
    }
}
