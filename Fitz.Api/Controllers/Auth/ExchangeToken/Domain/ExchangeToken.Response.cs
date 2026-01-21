namespace Fitz.Api.Controllers.Auth.ExchangeToken.Domain;

public record ExchangeTokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string? RefreshToken,
    string Scope)
{
    public static ExchangeTokenResponse From(ExchangeTokenModel model)
    {
        return new ExchangeTokenResponse(
            AccessToken: model.AccessToken,
            TokenType: model.TokenType,
            ExpiresIn: model.ExpiresIn,
            RefreshToken: model.RefreshToken,
            Scope: model.Scope
        );
    }
}
