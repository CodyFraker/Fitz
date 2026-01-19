using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain;

public record GetCurrentLotteryModel(
    int Id,
    DateTime StartDate,
    DateTime EndDate,
    int? Pool,
    int? WinningTicket,
    int TotalTickets,
    int TotalParticipants,
    double Odds)
{
    public static GetCurrentLotteryModel From(LotteryEntity lottery, int totalTickets, int totalParticipants)
    {
        double odds = totalTickets > 0 ? (1.0 / totalTickets) * 100 : 0;
        
        return new GetCurrentLotteryModel(
            Id: lottery.Id,
            StartDate: lottery.StartDate,
            EndDate: lottery.EndDate,
            Pool: lottery.Pool,
            WinningTicket: lottery.WinningTicket,
            TotalTickets: totalTickets,
            TotalParticipants: totalParticipants,
            Odds: odds
        );
    }
}
