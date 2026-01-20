namespace Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;

public class CalculateRenameCostFacade(CalculateRenameCostService calculateRenameCostService, ILogger<CalculateRenameCostFacade> logger)
{
    private readonly CalculateRenameCostService _calculateRenameCostService = calculateRenameCostService;
    private readonly ILogger<CalculateRenameCostFacade> _logger = logger;

    public async Task<CalculateRenameCostResponse> Execute(CalculateRenameCostCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CalculateRenameCostFacade execution started. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}", 
            command.AffectedUserId, command.RequestedUserId);

        var model = await _calculateRenameCostService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("CalculateRenameCostService execution completed. Cost: {Cost}", model.Cost);

        var response = CalculateRenameCostResponse.From(model);

        _logger.LogInformation("CalculateRenameCostFacade execution completed successfully. Cost: {Cost}", model.Cost);

        return response;
    }
}
