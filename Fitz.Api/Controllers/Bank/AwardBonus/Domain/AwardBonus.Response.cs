namespace Fitz.Api.Controllers.Bank.AwardBonus.Domain;

public record AwardBonusResponse(
    ulong UserId,
    int Amount,
    int NewBalance)
{
    public static AwardBonusResponse From(AwardBonusModel model)
    {
        return new AwardBonusResponse(
            UserId: model.Account.Id,
            Amount: model.Amount,
            NewBalance: model.Account.Beer
        );
    }
}
