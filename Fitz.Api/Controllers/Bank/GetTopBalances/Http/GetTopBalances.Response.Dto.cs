using Fitz.Api.Controllers.Bank.GetTopBalances.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Bank.GetTopBalances.Http;

[DisplayName("GetTopBalancesResponse")]
public record GetTopBalancesResponseDto
{
    [Required]
    public required List<AccountBalanceResponse> Accounts { get; set; }

    public static GetTopBalancesResponseDto From(GetTopBalancesResponse response)
    {
        return new GetTopBalancesResponseDto
        {
            Accounts = response.Accounts.Select(a => new AccountBalanceResponse
            {
                Id = a.Id,
                Username = a.Username,
                Beer = a.Beer
            }).ToList()
        };
    }
}
