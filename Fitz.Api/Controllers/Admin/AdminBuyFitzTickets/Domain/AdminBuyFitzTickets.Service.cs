using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Features.Lottery;

namespace Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Domain;

public class AdminBuyFitzTicketsService(
    IAdminBuyFitzTickets adminBuyFitzTickets,
    LotteryService lotteryService,
    ILogger<AdminBuyFitzTicketsService> logger)
{
    private readonly IAdminBuyFitzTickets _adminBuyFitzTickets = adminBuyFitzTickets;
    private readonly LotteryService _lotteryService = lotteryService;
    private readonly ILogger<AdminBuyFitzTicketsService> _logger = logger;

    public async Task<AdminBuyFitzTicketsModel> ExecuteAsync(AdminBuyFitzTicketsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminBuyFitzTicketsService execution started. Tickets: {Tickets}", command.Tickets);

        if (command.Tickets <= 0)
        {
            _logger.LogError("AdminBuyFitzTickets validation failed - Tickets must be greater than 0. Tickets: {Tickets}", command.Tickets);
            throw new ArgumentException("Tickets must be greater than 0.", nameof(command.Tickets));
        }

        var currentLottery = await _adminBuyFitzTickets.GetCurrentLotteryAsync(cancellationToken);
        if (currentLottery == null)
        {
            _logger.LogWarning("Current lottery not found");
            throw new LotteryNotFound();
        }

        var result = await _lotteryService.BuyTicketsForFitz(command.Tickets);
        if (!result.Success)
        {
            _logger.LogError("Failed to buy tickets for Fitz. Message: {Message}", result.Message);
            throw new InvalidOperationException(result.Message);
        }

        var model = AdminBuyFitzTicketsModel.From(result.Message);

        _logger.LogInformation("AdminBuyFitzTicketsModel created successfully. Message: {Message}", result.Message);

        return model;
    }
}
