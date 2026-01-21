using Fitz.Api.Controllers.Account.SetSafeBalance.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Account.SetSafeBalance.Http;

[DisplayName("SetSafeBalanceRequest")]
public record SetSafeBalanceRequestDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public required int SafeBalance { get; set; }

    internal SetSafeBalanceCommand ToCommand()
    {
        return SetSafeBalanceCommand.From(this);
    }
}
