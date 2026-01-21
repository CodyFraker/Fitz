using Fitz.Api.Controllers.Auth.ExchangeToken.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Auth.ExchangeToken.Http;

[DisplayName("ExchangeTokenRequest")]
public record ExchangeTokenRequestDto
{
    [Required]
    public required string Code { get; set; }

    [Required]
    public required string RedirectUri { get; set; }

    internal ExchangeTokenCommand ToCommand()
    {
        return ExchangeTokenCommand.From(this);
    }
}
