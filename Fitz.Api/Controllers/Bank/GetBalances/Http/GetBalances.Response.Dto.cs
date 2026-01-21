using Fitz.Api.Controllers.Bank.GetBalances.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Bank.GetBalances.Http;

[DisplayName("GetBalancesResponse")]
public record GetBalancesResponseDto
{
    [Required]
    public required List<AccountBalanceResponse> Accounts { get; set; }

    [Required]
    public required int TotalCount { get; set; }

    [Required]
    public required int Skip { get; set; }

    [Required]
    public required int Take { get; set; }

    public static GetBalancesResponseDto From(GetBalancesResponse response, int skip, int take)
    {
        return new GetBalancesResponseDto
        {
            Accounts = response.Accounts.Select(a => new AccountBalanceResponse
            {
                Id = a.Id,
                Username = a.Username,
                Beer = a.Beer
            }).ToList(),
            TotalCount = response.TotalCount,
            Skip = skip,
            Take = take
        };
    }
}
