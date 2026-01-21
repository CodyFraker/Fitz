using Fitz.Api.Controllers.Account.SetLotterySubscribe.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Account.SetLotterySubscribe.Http;

[DisplayName("SetLotterySubscribeResponse")]
public record SetLotterySubscribeResponseDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    public required bool Subscribe { get; set; }

    public static SetLotterySubscribeResponseDto From(SetLotterySubscribeResponse response)
    {
        return new SetLotterySubscribeResponseDto
        {
            UserId = response.UserId,
            Subscribe = response.Subscribe
        };
    }
}
