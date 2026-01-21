namespace Fitz.Api.Controllers.Account.SetLotterySubscribe.Domain;

public record SetLotterySubscribeResponse(
    ulong UserId,
    bool Subscribe)
{
    public static SetLotterySubscribeResponse From(SetLotterySubscribeModel model)
    {
        return new SetLotterySubscribeResponse(
            UserId: model.Account.Id,
            Subscribe: model.Subscribe
        );
    }
}
