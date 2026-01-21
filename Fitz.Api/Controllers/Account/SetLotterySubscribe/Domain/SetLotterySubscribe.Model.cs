using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Account.SetLotterySubscribe.Domain;

public record SetLotterySubscribeModel(
    AccountEntity Account,
    bool Subscribe)
{
    public static SetLotterySubscribeModel From(AccountEntity account, bool subscribe)
    {
        return new SetLotterySubscribeModel(
            Account: account,
            Subscribe: subscribe
        );
    }
}
