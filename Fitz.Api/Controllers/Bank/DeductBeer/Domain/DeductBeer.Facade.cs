namespace Fitz.Api.Controllers.Bank.DeductBeer.Domain;

public class DeductBeerFacade(DeductBeerService deductBeerService, ILogger<DeductBeerFacade> logger)
{
    private readonly DeductBeerService _deductBeerService = deductBeerService;
    private readonly ILogger<DeductBeerFacade> _logger = logger;

    public async Task<DeductBeerResponse> Execute(DeductBeerCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DeductBeerFacade execution started. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        var model = await _deductBeerService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("DeductBeerService execution completed. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        var response = DeductBeerResponse.From(model);

        _logger.LogInformation("DeductBeerFacade execution completed successfully. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        return response;
    }
}
