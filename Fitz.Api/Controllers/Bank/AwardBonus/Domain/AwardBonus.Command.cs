using Fitz.Api.Controllers.Bank.AwardBonus.Http;

namespace Fitz.Api.Controllers.Bank.AwardBonus.Domain;

public record AwardBonusCommand(ulong UserId, int Amount)
{
    public static AwardBonusCommand From(AwardBonusRequestDto request)
    {
        return new AwardBonusCommand(UserId: request.UserId, Amount: request.Amount);
    }
}
