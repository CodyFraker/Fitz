using Fitz.Api.Controllers.Auth.GetCurrentUser.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Auth.GetCurrentUser.Http;

[DisplayName("GetCurrentUserResponse")]
public record GetCurrentUserResponseDto
{
    [Required]
    public required ulong Id { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required]
    public required bool IsAdmin { get; set; }

    public static GetCurrentUserResponseDto From(GetCurrentUserResponse response)
    {
        return new GetCurrentUserResponseDto
        {
            Id = response.Id,
            Username = response.Username,
            IsAdmin = response.IsAdmin
        };
    }

    public CurrentUserResponse ToCurrentUserResponse()
    {
        return new CurrentUserResponse
        {
            Id = Id,
            Username = Username,
            IsAdmin = IsAdmin
        };
    }
}
