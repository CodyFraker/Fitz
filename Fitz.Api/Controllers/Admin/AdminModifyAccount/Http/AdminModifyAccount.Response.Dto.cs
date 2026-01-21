using Fitz.Api.Controllers.Admin.AdminModifyAccount.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Admin.AdminModifyAccount.Http;

[DisplayName("AdminModifyAccountResponse")]
public record AdminModifyAccountResponseDto
{
    [Required]
    public required AccountResponse Account { get; set; }

    public static AdminModifyAccountResponseDto From(AdminModifyAccountResponse response)
    {
        return new AdminModifyAccountResponseDto
        {
            Account = new AccountResponse
            {
                Id = response.Account.Id,
                Username = response.Account.Username,
                Beer = response.Account.Beer,
                LifetimeBeer = response.Account.LifetimeBeer,
                SafeBalance = response.Account.safeBalance,
                Favorability = response.Account.Favorability,
                CreatedDate = response.Account.CreatedDate,
                SubscribeToLottery = response.Account.subscribeToLottery,
                SubscribeTickets = response.Account.SubscribeTickets,
                Deactivated = response.Account.Deactivated
            }
        };
    }
}
