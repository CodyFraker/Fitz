namespace Fitz.Api.Controllers.Bank.TransferBeer.Domain;

public class TransferBeerFacade(TransferBeerService transferBeerService, ILogger<TransferBeerFacade> logger)
{
    private readonly TransferBeerService _transferBeerService = transferBeerService;
    private readonly ILogger<TransferBeerFacade> _logger = logger;

    public async Task<TransferBeerResponse> Execute(TransferBeerCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("TransferBeerFacade execution started. SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount}", command.SenderId, command.RecipientId, command.Amount);

        var model = await _transferBeerService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("TransferBeerService execution completed. SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount}", command.SenderId, command.RecipientId, command.Amount);

        var response = TransferBeerResponse.From(model);

        _logger.LogInformation("TransferBeerFacade execution completed successfully. SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount}", command.SenderId, command.RecipientId, command.Amount);

        return response;
    }
}
