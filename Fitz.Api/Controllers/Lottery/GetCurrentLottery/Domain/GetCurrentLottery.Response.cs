namespace Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain;

public record GetCurrentLotteryResponse(
    int Id,
    DateTime StartDate,
    DateTime EndDate,
    int? Pool,
    int? WinningTicket,
    int TotalTickets,
    int TotalParticipants,
    double Odds)
{
    public static GetCurrentLotteryResponse From(GetCurrentLotteryModel model)
    {
        return new GetCurrentLotteryResponse(
            Id: model.Id,
            StartDate: model.StartDate,
            EndDate: model.EndDate,
            Pool: model.Pool,
            WinningTicket: model.WinningTicket,
            TotalTickets: model.TotalTickets,
            TotalParticipants: model.TotalParticipants,
            Odds: model.Odds
        );
    }
}
