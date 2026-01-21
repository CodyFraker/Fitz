using Fitz.Api.Controllers.Account.DeactivateAccount.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Account.DeactivateAccount.Http;

[DisplayName("DeactivateAccountRequest")]
public record DeactivateAccountRequestDto
{
    [Required]
    public required ulong UserId { get; set; }

    internal DeactivateAccountCommand ToCommand()
    {
        return DeactivateAccountCommand.From(this);
    }
}
