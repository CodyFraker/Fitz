using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Database.Entities;
using Fitz.Features.Bank;
using Fitz.Features.Rename;

namespace Fitz.Api.Controllers.Rename.CreateRename.Domain;

public class CreateRenameService(ICreateRename createRename, RenameService renameService, BankService bankService, ILogger<CreateRenameService> logger)
{
    private readonly ICreateRename _createRename = createRename;
    private readonly RenameService _renameService = renameService;
    private readonly BankService _bankService = bankService;
    private readonly ILogger<CreateRenameService> _logger = logger;

    public async Task<CreateRenameModel> ExecuteAsync(CreateRenameCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CreateRenameService execution started. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}", command.AffectedUserId, command.RequestedUserId);

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

        var cost = _renameService.GenerateRenameCost(affectedUser, requestedUser, command.Days, command.NewName);

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

        await _bankService.PurchaseRenameAsync(createdRename.RequestedUserId, createdRename.Cost);

        _logger.LogInformation("CreateRenameService execution completed. RenameId: {RenameId}", createdRename.Id);

        return CreateRenameModel.From(createdRename);
    }
}
