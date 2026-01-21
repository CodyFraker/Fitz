using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Database.Entities;

namespace Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;

public class CalculateRenameCostService(ICalculateRenameCost calculateRenameCost, ILogger<CalculateRenameCostService> logger)
{
    private readonly ICalculateRenameCost _calculateRenameCost = calculateRenameCost;
    private readonly ILogger<CalculateRenameCostService> _logger = logger;

    public async Task<CalculateRenameCostModel> ExecuteAsync(CalculateRenameCostCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CalculateRenameCostService execution started. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}, Days: {Days}, NewName: {NewName}", 
            command.AffectedUserId, command.RequestedUserId, command.Days, command.NewName);

        if (command.AffectedUserId == 0)
        {
            _logger.LogError("CalculateRenameCost validation failed - Affected User ID cannot be 0.");
            throw new ArgumentException("Affected User ID cannot be 0.", nameof(command.AffectedUserId));
        }

        if (command.RequestedUserId == 0)
        {
            _logger.LogError("CalculateRenameCost validation failed - Requested User ID cannot be 0.");
            throw new ArgumentException("Requested User ID cannot be 0.", nameof(command.RequestedUserId));
        }

        if (command.Days <= 0)
        {
            _logger.LogError("CalculateRenameCost validation failed - Days must be greater than 0. Days: {Days}", command.Days);
            throw new ArgumentException("Days must be greater than 0.", nameof(command.Days));
        }

        if (string.IsNullOrWhiteSpace(command.NewName))
        {
            _logger.LogError("CalculateRenameCost validation failed - NewName cannot be empty.");
            throw new ArgumentException("NewName cannot be empty.", nameof(command.NewName));
        }

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

        var settings = await _calculateRenameCost.GetSettingsAsync(cancellationToken);
        if (settings == null)
        {
            _logger.LogError("Settings not found");
            throw new InvalidOperationException("Settings not found");
        }

        var cost = CalculateCost(affectedUser, requestedUser, command.Days, command.NewName, settings);

        var model = CalculateRenameCostModel.From(cost);

        _logger.LogInformation("CalculateRenameCostModel created successfully. Cost: {Cost}", cost);

        return model;
    }

    private int CalculateCost(AccountEntity affectedUser, AccountEntity requestedUser, double daysOfRename, string newName, SettingsEntity settings)
    {
        if (affectedUser.Username == "Fitz")
        {
            return 999999999;
        }

        double baseCost = (double)settings.RenameBaseCost;

        if (affectedUser.Id == requestedUser.Id)
        {
            baseCost += 150;
        }

        if (affectedUser.Favorability <= 5)
        {
            baseCost *= 1;
        }
        if (affectedUser.Favorability <= 20 && affectedUser.Favorability >= 6)
        {
            baseCost *= 0.8;
        }
        if (affectedUser.Favorability <= 40 && affectedUser.Favorability >= 21)
        {
            baseCost *= 0.6;
        }
        if (affectedUser.Favorability <= 60 && affectedUser.Favorability >= 41)
        {
            baseCost *= 0.4;
        }
        if (affectedUser.Favorability <= 80 && affectedUser.Favorability >= 61)
        {
            baseCost *= 0.2;
        }

        if (requestedUser.Favorability == 0)
        {
            baseCost *= 100;
        }
        if (requestedUser.Favorability <= 20 && requestedUser.Favorability >= 6)
        {
            baseCost /= 0.1;
        }
        if (requestedUser.Favorability <= 40 && requestedUser.Favorability >= 21)
        {
            baseCost *= 0.2;
        }
        if (requestedUser.Favorability <= 60 && requestedUser.Favorability >= 41)
        {
            baseCost *= 0.4;
        }
        if (requestedUser.Favorability <= 80 && requestedUser.Favorability >= 61)
        {
            baseCost *= 0.8;
        }

        foreach (char c in newName)
        {
            baseCost *= 1.2;
        }

        return int.Parse(Math.Ceiling(baseCost * daysOfRename).ToString());
    }
}
