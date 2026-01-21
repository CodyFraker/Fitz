using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.GetBalance.Domain;

public record GetBalanceModel(
    AccountEntity Account)
{
    public static GetBalanceModel From(AccountEntity account)
    {
        return new GetBalanceModel(Account: account);
    }
}
