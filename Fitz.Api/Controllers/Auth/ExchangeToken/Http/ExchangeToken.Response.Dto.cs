using Fitz.Api.Controllers.Auth.ExchangeToken.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Auth.ExchangeToken.Http;

[DisplayName("ExchangeTokenResponse")]
public record ExchangeTokenResponseDto
{
    [Required]
    public required string AccessToken { get; set; }

    [Required]
    public required string TokenType { get; set; }

    [Required]
    public required int ExpiresIn { get; set; }

    public string? RefreshToken { get; set; }

    [Required]
    public required string Scope { get; set; }

    public static ExchangeTokenResponseDto From(ExchangeTokenResponse response)
    {
        return new ExchangeTokenResponseDto
        {
            AccessToken = response.AccessToken,
            TokenType = response.TokenType,
            ExpiresIn = response.ExpiresIn,
            RefreshToken = response.RefreshToken,
            Scope = response.Scope
        };
    }

    public AuthTokenResponse ToAuthTokenResponse()
    {
        return new AuthTokenResponse
        {
            AccessToken = AccessToken,
            TokenType = TokenType,
            ExpiresIn = ExpiresIn,
            RefreshToken = RefreshToken,
            Scope = Scope
        };
    }
}
