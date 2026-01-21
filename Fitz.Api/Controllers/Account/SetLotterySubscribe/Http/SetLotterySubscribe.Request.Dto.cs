using Fitz.Api.Controllers.Account.SetLotterySubscribe.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Account.SetLotterySubscribe.Http;

[DisplayName("SetLotterySubscribeRequest")]
public record SetLotterySubscribeRequestDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    public required bool Subscribe { get; set; }

    internal SetLotterySubscribeCommand ToCommand()
    {
        return SetLotterySubscribeCommand.From(this);
    }
}
