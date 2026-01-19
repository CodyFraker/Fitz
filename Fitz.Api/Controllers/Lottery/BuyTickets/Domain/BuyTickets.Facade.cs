namespace Fitz.Api.Controllers.Lottery.BuyTickets.Domain;

public class BuyTicketsFacade(BuyTicketsService buyTicketsService, ILogger<BuyTicketsFacade> logger)
{
    private readonly BuyTicketsService _buyTicketsService = buyTicketsService;
    private readonly ILogger<BuyTicketsFacade> _logger = logger;

    public async Task<BuyTicketsResponse> Execute(BuyTicketsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("BuyTicketsFacade execution started. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        var model = await _buyTicketsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("BuyTicketsService execution completed. UserId: {UserId}, TicketsPurchased: {TicketsPurchased}", 
            command.UserId, model.TicketsPurchased);

        var response = BuyTicketsResponse.From(model);

        _logger.LogInformation("BuyTicketsFacade execution completed successfully. UserId: {UserId}, TicketsPurchased: {TicketsPurchased}", 
            command.UserId, model.TicketsPurchased);

        return response;
    }
}
