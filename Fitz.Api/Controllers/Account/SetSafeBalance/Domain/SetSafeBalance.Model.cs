using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.SetSafeBalance.Domain;

public record SetSafeBalanceModel(
    AccountEntity Account,
    int SafeBalance)
{
    public static SetSafeBalanceModel From(AccountEntity account, int safeBalance)
    {
        return new SetSafeBalanceModel(
            Account: account,
            SafeBalance: safeBalance
        );
    }
}
