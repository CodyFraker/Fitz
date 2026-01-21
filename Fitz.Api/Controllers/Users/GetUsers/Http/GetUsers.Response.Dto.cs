using Fitz.Api.Controllers.Users.GetUsers.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Users.GetUsers.Http;

[DisplayName("GetUsersResponse")]
public record GetUsersResponseDto
{
    [Required]
    public required List<UserResponse> Users { get; set; }

    [Required]
    public required int TotalCount { get; set; }

    [Required]
    public required int Page { get; set; }

    [Required]
    public required int PageSize { get; set; }

    [Required]
    public required int TotalPages { get; set; }

    public static GetUsersResponseDto From(GetUsersResponse response)
    {
        return new GetUsersResponseDto
        {
            Users = response.Accounts.Select(a => new UserResponse
            {
                Id = a.Id,
                Username = a.Username
            }).ToList(),
            TotalCount = response.TotalCount,
            Page = response.Page,
            PageSize = response.PageSize,
            TotalPages = response.TotalPages
        };
    }
}
