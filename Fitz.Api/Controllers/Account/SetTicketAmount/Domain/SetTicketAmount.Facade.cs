namespace Fitz.Api.Controllers.Account.SetTicketAmount.Domain;

public class SetTicketAmountFacade(SetTicketAmountService setTicketAmountService, ILogger<SetTicketAmountFacade> logger)
{
    private readonly SetTicketAmountService _setTicketAmountService = setTicketAmountService;
    private readonly ILogger<SetTicketAmountFacade> _logger = logger;

    public async Task<SetTicketAmountResponse> Execute(SetTicketAmountCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SetTicketAmountFacade execution started. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        var model = await _setTicketAmountService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("SetTicketAmountService execution completed. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        var response = SetTicketAmountResponse.From(model);

        _logger.LogInformation("SetTicketAmountFacade execution completed successfully. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        return response;
    }
}
