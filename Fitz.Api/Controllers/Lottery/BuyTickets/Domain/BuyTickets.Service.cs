using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Lottery.Exceptions;
using Fitz.Features.Bank;
using Fitz.Metrics;

namespace Fitz.Api.Controllers.Lottery.BuyTickets.Domain;

public class BuyTicketsService(IBuyTickets buyTickets, BankService bankService, FitzMetrics? fitzMetrics, ILogger<BuyTicketsService> logger)
{
    private readonly IBuyTickets _buyTickets = buyTickets;
    private readonly BankService _bankService = bankService;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;
    private readonly ILogger<BuyTicketsService> _logger = logger;

    public async Task<BuyTicketsModel> ExecuteAsync(BuyTicketsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("BuyTicketsService execution started. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        if (command.Amount <= 0)
        {
            _logger.LogError("BuyTickets validation failed - Amount must be greater than 0. Amount: {Amount}", command.Amount);
            throw new InvalidTicketAmountException(command.Amount, "Amount must be greater than 0");
        }

        var account = await _buyTickets.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        var settings = await _buyTickets.GetSettingsAsync(cancellationToken);
        if (settings == null || settings.MaxTickets == 0)
        {
            _logger.LogError("Failed to get lottery settings or settings invalid");
            throw new InvalidOperationException("Failed to get lottery settings or settings invalid");
        }

        var lottery = await _buyTickets.GetCurrentLotteryAsync(cancellationToken);
        if (lottery == null)
        {
            _logger.LogWarning("Current lottery not found");
            throw new LotteryNotFound();
        }

        var userTickets = await _buyTickets.GetUserTicketsAsync(command.UserId, lottery.Id, cancellationToken);

        if (userTickets.Count >= settings.MaxTickets)
        {
            _logger.LogWarning("User already has max amount of tickets. UserId: {UserId}, CurrentTickets: {CurrentTickets}, MaxTickets: {MaxTickets}", 
                command.UserId, userTickets.Count, settings.MaxTickets);
            throw new MaxTicketsReachedException(userTickets.Count, settings.MaxTickets);
        }

        int totalBuyableTickets = settings.MaxTickets - userTickets.Count;
        int ticketsToBuy = command.Amount;
        if (ticketsToBuy > totalBuyableTickets)
        {
            if (totalBuyableTickets > 0)
            {
                ticketsToBuy = totalBuyableTickets;
            }
            else
            {
                _logger.LogWarning("No tickets can be bought. UserId: {UserId}, Requested: {Requested}, Available: {Available}", 
                    command.UserId, command.Amount, totalBuyableTickets);
                throw new InvalidTicketAmountException(command.Amount, $"Only {totalBuyableTickets} tickets can be bought");
            }
        }

        int totalTicketCost = ticketsToBuy * settings.TicketCost;
        if (account.Beer < totalTicketCost)
        {
            _logger.LogWarning("User does not have enough beer. UserId: {UserId}, Required: {Required}, Current: {Current}", 
                command.UserId, totalTicketCost, account.Beer);
            throw new InsufficientBeerException(totalTicketCost, account.Beer);
        }

        var createdTickets = await _buyTickets.CreateTicketsAsync(command.UserId, lottery.Id, ticketsToBuy, cancellationToken);

        var purchaseResult = await _bankService.PurchaseLotteryTicket(account, totalTicketCost);
        if (!purchaseResult.Success)
        {
            _logger.LogError("Failed to purchase lottery tickets. UserId: {UserId}, Message: {Message}", 
                command.UserId, purchaseResult.Message);
            throw new InvalidOperationException($"Failed to purchase lottery tickets: {purchaseResult.Message}");
        }

        await _buyTickets.AddToPoolAsync(lottery.Id, totalTicketCost, cancellationToken);

        _fitzMetrics?.RecordLotteryTicketPurchase(ticketsToBuy, totalTicketCost);

        var model = BuyTicketsModel.From(createdTickets, totalTicketCost, ticketsToBuy);

        _logger.LogInformation("BuyTicketsModel created successfully. UserId: {UserId}, TicketsPurchased: {TicketsPurchased}, TotalCost: {TotalCost}", 
            command.UserId, ticketsToBuy, totalTicketCost);

        return model;
    }
}
