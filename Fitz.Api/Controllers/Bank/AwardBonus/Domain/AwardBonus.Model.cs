using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Bank.AwardBonus.Domain;

public record AwardBonusModel(
    AccountEntity Account,
    int Amount)
{
    public static AwardBonusModel From(AccountEntity account, int amount)
    {
        return new AwardBonusModel(Account: account, Amount: amount);
    }
}
