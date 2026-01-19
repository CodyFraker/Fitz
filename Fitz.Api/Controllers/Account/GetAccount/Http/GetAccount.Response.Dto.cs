using Fitz.Api.Controllers.Account.GetAccount.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Account.GetAccount.Http
{
    [DisplayName("GetAccountResponse")]
    public record GetAccountResponseDto
    {
        [Required]
        public required ulong Id { get; set; }

        public string? Username { get; set; }

        [Required]
        public required int Beer { get; set; }

        [Required]
        public required int LifetimeBeer { get; set; }

        [Required]
        public required int SafeBalance { get; set; }

        [Required]
        public required int Favorability { get; set; }

        [Required]
        public required DateTime CreatedDate { get; set; }

        [Required]
        public required bool SubscribeToLottery { get; set; }

        [Required]
        public required int SubscribeTickets { get; set; }

        [Required]
        public required bool Deactivated { get; set; }

        public static GetAccountResponseDto From(GetAccountResponse response)
        {
            return new GetAccountResponseDto
            {
                Id = response.Id,
                Username = response.Username,
                Beer = response.Beer,
                LifetimeBeer = response.LifetimeBeer,
                SafeBalance = response.SafeBalance,
                Favorability = response.Favorability,
                CreatedDate = response.CreatedDate,
                SubscribeToLottery = response.SubscribeToLottery,
                SubscribeTickets = response.SubscribeTickets,
                Deactivated = response.Deactivated
            };
        }
    }
}
