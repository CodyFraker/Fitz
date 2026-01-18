namespace Fitz.Api.Models.Responses
{
    public class CurrentLotteryResponse
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Pool { get; set; }
        public int TotalTickets { get; set; }
        public int TotalParticipants { get; set; }
        public double Odds { get; set; }
        public int? WinningTicket { get; set; }
    }

    public class LotteryHistoryItem
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Pool { get; set; }
        public int? WinningTicket { get; set; }
        public int TotalTickets { get; set; }
        public int TotalParticipants { get; set; }
    }

    public class LotteryHistoryResponse
    {
        public List<LotteryHistoryItem> Lotteries { get; set; } = new();
        public int TotalCount { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }

    public class LotteryStatisticsPoint
    {
        public DateTime Date { get; set; }
        public int PrizePool { get; set; }
        public int TotalTickets { get; set; }
    }

    public class LotteryStatisticsResponse
    {
        public List<LotteryStatisticsPoint> DataPoints { get; set; } = new();
    }
}
