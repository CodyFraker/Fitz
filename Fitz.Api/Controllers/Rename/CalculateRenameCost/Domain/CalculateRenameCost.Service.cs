using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Database.Entities;
using Fitz.Features.Rename;

namespace Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;

public class CalculateRenameCostService(ICalculateRenameCost calculateRenameCost, RenameService renameService, ILogger<CalculateRenameCostService> logger)
{
    private readonly ICalculateRenameCost _calculateRenameCost = calculateRenameCost;
    private readonly RenameService _renameService = renameService;
    private readonly ILogger<CalculateRenameCostService> _logger = logger;

    public async Task<CalculateRenameCostModel> ExecuteAsync(CalculateRenameCostCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CalculateRenameCostService execution started. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}", 
            command.AffectedUserId, command.RequestedUserId);

        var affectedUser = await _calculateRenameCost.FindAccountByIdAsync(command.AffectedUserId, cancellationToken);
        if (affectedUser == null)
        {
            _logger.LogWarning("Affected user account not found. AffectedUserId: {AffectedUserId}", command.AffectedUserId);
            throw new AccountNotFound(command.AffectedUserId);
        }

        var requestedUser = await _calculateRenameCost.FindAccountByIdAsync(command.RequestedUserId, cancellationToken);
        if (requestedUser == null)
        {
            _logger.LogWarning("Requested user account not found. RequestedUserId: {RequestedUserId}", command.RequestedUserId);
            throw new AccountNotFound(command.RequestedUserId);
        }

        var cost = _renameService.GenerateRenameCost(affectedUser, requestedUser, command.Days, command.NewName);

        _logger.LogInformation("CalculateRenameCostService execution completed. Cost: {Cost}", cost);

        return CalculateRenameCostModel.From(cost);
    }
}
