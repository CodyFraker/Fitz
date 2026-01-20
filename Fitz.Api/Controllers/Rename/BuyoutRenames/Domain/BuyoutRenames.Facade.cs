namespace Fitz.Api.Controllers.Rename.BuyoutRenames.Domain;

public class BuyoutRenamesFacade(BuyoutRenamesService buyoutRenamesService, ILogger<BuyoutRenamesFacade> logger)
{
    private readonly BuyoutRenamesService _buyoutRenamesService = buyoutRenamesService;
    private readonly ILogger<BuyoutRenamesFacade> _logger = logger;

    public async Task<BuyoutRenamesResponse> Execute(BuyoutRenamesCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("BuyoutRenamesFacade execution started. UserId: {UserId}", command.UserId);

        var model = await _buyoutRenamesService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("BuyoutRenamesService execution completed. UserId: {UserId}, Count: {Count}", command.UserId, model.RenamesUpdated);

        var response = BuyoutRenamesResponse.From(model);

        _logger.LogInformation("BuyoutRenamesFacade execution completed successfully. UserId: {UserId}, Count: {Count}", command.UserId, model.RenamesUpdated);

        return response;
    }
}
