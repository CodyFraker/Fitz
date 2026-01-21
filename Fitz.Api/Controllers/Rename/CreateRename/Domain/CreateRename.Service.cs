using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Rename.Exceptions;
using Fitz.Database.Entities;
using Fitz.Features.Bank;
using Fitz.Metrics;

namespace Fitz.Api.Controllers.Rename.CreateRename.Domain;

public class CreateRenameService(
    ICreateRename createRename,
    BankService bankService,
    FitzMetrics? fitzMetrics,
    ILogger<CreateRenameService> logger)
{
    private readonly ICreateRename _createRename = createRename;
    private readonly BankService _bankService = bankService;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;
    private readonly ILogger<CreateRenameService> _logger = logger;

    public async Task<CreateRenameModel> ExecuteAsync(CreateRenameCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CreateRenameService execution started. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}, Days: {Days}, NewName: {NewName}", 
            command.AffectedUserId, command.RequestedUserId, command.Days, command.NewName);

        if (command.AffectedUserId == 0)
        {
            _logger.LogError("CreateRename validation failed - Affected User ID cannot be 0.");
            throw new ArgumentException("Affected User ID cannot be 0.", nameof(command.AffectedUserId));
        }

        if (command.RequestedUserId == 0)
        {
            _logger.LogError("CreateRename validation failed - Requested User ID cannot be 0.");
            throw new ArgumentException("Requested User ID cannot be 0.", nameof(command.RequestedUserId));
        }

        if (command.Days <= 0)
        {
            _logger.LogError("CreateRename validation failed - Days must be greater than 0. Days: {Days}", command.Days);
            throw new ArgumentException("Days must be greater than 0.", nameof(command.Days));
        }

        if (string.IsNullOrWhiteSpace(command.NewName))
        {
            _logger.LogError("CreateRename validation failed - NewName cannot be empty.");
            throw new ArgumentException("NewName cannot be empty.", nameof(command.NewName));
        }

        var affectedUser = await _createRename.FindAccountByIdAsync(command.AffectedUserId, cancellationToken);
        if (affectedUser == null)
        {
            _logger.LogWarning("Affected user account not found. AffectedUserId: {AffectedUserId}", command.AffectedUserId);
            throw new AccountNotFound(command.AffectedUserId);
        }

        var requestedUser = await _createRename.FindAccountByIdAsync(command.RequestedUserId, cancellationToken);
        if (requestedUser == null)
        {
            _logger.LogWarning("Requested user account not found. RequestedUserId: {RequestedUserId}", command.RequestedUserId);
            throw new AccountNotFound(command.RequestedUserId);
        }

        var settings = await _createRename.GetSettingsAsync(cancellationToken);
        if (settings == null)
        {
            _logger.LogError("Settings not found");
            throw new InvalidOperationException("Settings not found");
        }

        var cost = CalculateCost(affectedUser, requestedUser, command.Days, command.NewName, settings);

        if (requestedUser.Beer < cost)
        {
            _logger.LogWarning("Insufficient beer. RequestedUserId: {RequestedUserId}, Required: {Required}, Current: {Current}", 
                command.RequestedUserId, cost, requestedUser.Beer);
            throw new InsufficientBeerException(cost, requestedUser.Beer);
        }

        var rename = new RenamesEntity
        {
            NewName = command.NewName,
            AffectedUserId = command.AffectedUserId,
            RequestedUserId = command.RequestedUserId,
            Days = command.Days,
            Cost = cost,
            Status = command.Status ?? RenameStatusEnum.Pending,
            StartDate = command.StartDate,
            Expiration = command.Expiration,
            Timestamp = DateTime.UtcNow
        };

        var createdRename = await _createRename.CreateRenameAsync(rename, cancellationToken);

        await _bankService.PurchaseRenameAsync(command.RequestedUserId, cost);

        _fitzMetrics?.RecordRenameCreated(cost);

        var activeRenames = await _createRename.GetRenamesByAccountIdAsync(command.AffectedUserId, cancellationToken);
        var activeCount = activeRenames.Where(r => r.Status == RenameStatusEnum.Active).Count();
        _fitzMetrics?.SetRenamesActive(activeCount);

        var savedRename = await _createRename.FindRenameAfterCreationAsync(
            command.AffectedUserId,
            command.RequestedUserId,
            command.NewName,
            rename.Timestamp,
            cancellationToken);

        if (savedRename == null)
        {
            _logger.LogError("Rename created but could not be retrieved. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}", 
                command.AffectedUserId, command.RequestedUserId);
            throw new InvalidOperationException("Rename created but could not be retrieved");
        }

        var model = CreateRenameModel.From(savedRename);

        _logger.LogInformation("CreateRenameModel created successfully. RenameId: {RenameId}", savedRename.Id);

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
