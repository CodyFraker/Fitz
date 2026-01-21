using Fitz.Api.Controllers.Account.SetSafeBalance.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Account.SetSafeBalance.Http;

[DisplayName("SetSafeBalanceResponse")]
public record SetSafeBalanceResponseDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    public required int SafeBalance { get; set; }

    public static SetSafeBalanceResponseDto From(SetSafeBalanceResponse response)
    {
        return new SetSafeBalanceResponseDto
        {
            UserId = response.UserId,
            SafeBalance = response.SafeBalance
        };
    }
}
