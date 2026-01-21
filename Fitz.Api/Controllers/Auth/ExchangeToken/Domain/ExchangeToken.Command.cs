using Fitz.Api.Controllers.Auth.ExchangeToken.Http;

namespace Fitz.Api.Controllers.Auth.ExchangeToken.Domain;

public record ExchangeTokenCommand(string Code, string RedirectUri)
{
    public static ExchangeTokenCommand From(ExchangeTokenRequestDto request)
    {
        return new ExchangeTokenCommand(Code: request.Code, RedirectUri: request.RedirectUri);
    }
}
