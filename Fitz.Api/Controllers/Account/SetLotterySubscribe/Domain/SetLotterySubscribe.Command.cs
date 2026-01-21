using Fitz.Api.Controllers.Account.SetLotterySubscribe.Http;

namespace Fitz.Api.Controllers.Account.SetLotterySubscribe.Domain;

public record SetLotterySubscribeCommand(ulong UserId, bool Subscribe)
{
    public static SetLotterySubscribeCommand From(SetLotterySubscribeRequestDto request)
    {
        return new SetLotterySubscribeCommand(UserId: request.UserId, Subscribe: request.Subscribe);
    }
}
