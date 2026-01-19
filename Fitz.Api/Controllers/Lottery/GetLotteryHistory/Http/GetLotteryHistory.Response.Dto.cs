using Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Lottery.GetLotteryHistory.Http;

[DisplayName("LotteryHistoryItem")]
public record LotteryHistoryItemDto
{
    [Required]
    public required int Id { get; set; }

    [Required]
    public required DateTime StartDate { get; set; }

    [Required]
    public required DateTime EndDate { get; set; }

    public int? Pool { get; set; }

    public int? WinningTicket { get; set; }

    [Required]
    public required int TotalTickets { get; set; }

    [Required]
    public required int TotalParticipants { get; set; }
}

[DisplayName("GetLotteryHistoryResponse")]
public record GetLotteryHistoryResponseDto
{
    [Required]
    public required List<LotteryHistoryItemDto> Lotteries { get; set; }

    [Required]
    public required int TotalCount { get; set; }

    [Required]
    public required int Skip { get; set; }

    [Required]
    public required int Take { get; set; }

    public static GetLotteryHistoryResponseDto From(GetLotteryHistoryResponse response)
    {
        return new GetLotteryHistoryResponseDto
        {
            Lotteries = response.Lotteries.Select(l => new LotteryHistoryItemDto
            {
                Id = l.Id,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Pool = l.Pool,
                WinningTicket = l.WinningTicket,
                TotalTickets = l.TotalTickets,
                TotalParticipants = l.TotalParticipants
            }).ToList(),
            TotalCount = response.TotalCount,
            Skip = response.Skip,
            Take = response.Take
        };
    }
}
