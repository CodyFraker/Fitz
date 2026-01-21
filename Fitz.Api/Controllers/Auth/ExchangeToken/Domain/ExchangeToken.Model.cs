namespace Fitz.Api.Controllers.Auth.ExchangeToken.Domain;

public record ExchangeTokenModel(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string? RefreshToken,
    string Scope)
{
    public static ExchangeTokenModel From(string accessToken, string tokenType, int expiresIn, string? refreshToken, string scope)
    {
        return new ExchangeTokenModel(
            AccessToken: accessToken,
            TokenType: tokenType,
            ExpiresIn: expiresIn,
            RefreshToken: refreshToken,
            Scope: scope
        );
    }
}
