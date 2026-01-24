namespace Fitz.Api.Controllers.Account.GiveBeer.Domain;

public class GiveBeerFacade(GiveBeerService giveBeerService, ILogger<GiveBeerFacade> logger)
{
    private readonly GiveBeerService _giveBeerService = giveBeerService;
    private readonly ILogger<GiveBeerFacade> _logger = logger;

    public async Task<GiveBeerModel> Execute(GiveBeerCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GiveBeerFacade execution started. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        var model = await _giveBeerService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GiveBeerFacade execution completed successfully. UserId: {UserId}, Amount: {Amount}, NewFavorability: {NewFavorability}", 
            command.UserId, command.Amount, model.NewFavorability);

        return model;
    }
}
