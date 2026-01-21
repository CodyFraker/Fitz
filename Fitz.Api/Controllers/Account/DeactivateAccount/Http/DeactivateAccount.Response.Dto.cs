using Fitz.Api.Controllers.Account.DeactivateAccount.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Account.DeactivateAccount.Http;

[DisplayName("DeactivateAccountResponse")]
public record DeactivateAccountResponseDto
{
    [Required]
    public required ulong UserId { get; set; }

    [Required]
    public required bool Deactivated { get; set; }

    public static DeactivateAccountResponseDto From(DeactivateAccountResponse response)
    {
        return new DeactivateAccountResponseDto
        {
            UserId = response.UserId,
            Deactivated = response.Deactivated
        };
    }
}
