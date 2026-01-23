namespace Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain;

public class GetLotteryHistoryService(IGetLotteryHistory getLotteryHistory, ILogger<GetLotteryHistoryService> logger)
{
    private readonly IGetLotteryHistory _getLotteryHistory = getLotteryHistory;
    private readonly ILogger<GetLotteryHistoryService> _logger = logger;

    public async Task<GetLotteryHistoryModel> ExecuteAsync(GetLotteryHistoryCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetLotteryHistoryService execution started. Skip: {Skip}, Take: {Take}", command.Skip, command.Take);

        var lotteries = await _getLotteryHistory.FindHistoryAsync(command.Skip, command.Take, cancellationToken);
        var totalCount = await _getLotteryHistory.GetTotalCountAsync(cancellationToken);

        var historyItems = new List<LotteryHistoryItemModel>();

        foreach (var lottery in lotteries)
        {
            var totalTickets = await _getLotteryHistory.GetTotalTicketsAsync(lottery.Id, cancellationToken);
            var totalParticipants = await _getLotteryHistory.GetTotalParticipantsAsync(lottery.Id, cancellationToken);
            var winnersData = await _getLotteryHistory.GetWinnersByDrawingIdAsync(lottery.Id, cancellationToken);

            var winners = winnersData.Select(w => new LotteryWinnerModel(
                AccountId: w.Winner.AccountId,
                Username: w.Account.Username,
                Payout: w.Winner.Payout
            )).ToList();

            var item = new LotteryHistoryItemModel(
                Id: lottery.Id,
                StartDate: lottery.StartDate,
                EndDate: lottery.EndDate,
                Pool: lottery.Pool,
                WinningTicket: lottery.WinningTicket,
                TotalTickets: totalTickets,
                TotalParticipants: totalParticipants,
                Winners: winners
            );

            historyItems.Add(item);
        }

        var model = GetLotteryHistoryModel.From(historyItems, totalCount, command.Skip, command.Take);

        _logger.LogInformation("GetLotteryHistoryModel created successfully. TotalCount: {TotalCount}, ItemsReturned: {ItemsReturned}", totalCount, historyItems.Count);

        return model;
    }
}
