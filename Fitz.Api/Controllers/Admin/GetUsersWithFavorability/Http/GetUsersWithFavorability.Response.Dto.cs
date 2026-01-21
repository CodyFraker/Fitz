using Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Http;

[DisplayName("GetUsersWithFavorabilityResponse")]
public record GetUsersWithFavorabilityResponseDto
{
    [Required]
    public required List<UserFavorabilityResponse> Users { get; set; }

    [Required]
    public required int TotalCount { get; set; }

    public static GetUsersWithFavorabilityResponseDto From(GetUsersWithFavorabilityResponse response)
    {
        return new GetUsersWithFavorabilityResponseDto
        {
            Users = response.Accounts.Select(account =>
            {
                decimal beerRatio = (decimal)account.Beer / response.BotBeer;
                return new UserFavorabilityResponse
                {
                    UserId = account.Id,
                    Username = account.Username ?? "Unknown",
                    Beer = account.Beer,
                    BotBeer = response.BotBeer,
                    BeerRatio = beerRatio,
                    Favorability = account.Favorability,
                    CanUseCommands = account.Favorability > 0
                };
            }).ToList(),
            TotalCount = response.TotalCount
        };
    }
}
