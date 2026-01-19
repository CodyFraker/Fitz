using Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Lottery.GetCurrentLottery.Http;

[DisplayName("GetCurrentLotteryResponse")]
public record GetCurrentLotteryResponseDto
{
    [Required]
    public required int Id { get; set; }

    [Required]
    public required DateTime StartDate { get; set; }

    [Required]
    public required DateTime EndDate { get; set; }

    public int? Pool { get; set; }

    [Required]
    public required int TotalTickets { get; set; }

    [Required]
    public required int TotalParticipants { get; set; }

    [Required]
    public required double Odds { get; set; }

    public int? WinningTicket { get; set; }

    public static GetCurrentLotteryResponseDto From(GetCurrentLotteryResponse response)
    {
        return new GetCurrentLotteryResponseDto
        {
            Id = response.Id,
            StartDate = response.StartDate,
            EndDate = response.EndDate,
            Pool = response.Pool,
            TotalTickets = response.TotalTickets,
            TotalParticipants = response.TotalParticipants,
            Odds = response.Odds,
            WinningTicket = response.WinningTicket
        };
    }
}
