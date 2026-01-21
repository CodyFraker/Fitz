using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Database.Entities;
using Fitz.Metrics;

namespace Fitz.Api.Controllers.Bank.TransferBeer.Domain;

public class TransferBeerService(ITransferBeer transferBeer, FitzMetrics? fitzMetrics, ILogger<TransferBeerService> logger)
{
    private readonly ITransferBeer _transferBeer = transferBeer;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;
    private readonly ILogger<TransferBeerService> _logger = logger;

    public async Task<TransferBeerModel> ExecuteAsync(TransferBeerCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("TransferBeerService execution started. SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount}", command.SenderId, command.RecipientId, command.Amount);

        if (command.SenderId == 0)
        {
            _logger.LogError("TransferBeer validation failed - Sender ID cannot be 0.");
            throw new ArgumentException("Sender ID cannot be 0.", nameof(command.SenderId));
        }

        if (command.RecipientId == 0)
        {
            _logger.LogError("TransferBeer validation failed - Recipient ID cannot be 0.");
            throw new ArgumentException("Recipient ID cannot be 0.", nameof(command.RecipientId));
        }

        if (command.SenderId == command.RecipientId)
        {
            _logger.LogError("TransferBeer validation failed - Sender and Recipient cannot be the same.");
            throw new ArgumentException("Sender and Recipient cannot be the same.", nameof(command.RecipientId));
        }

        if (command.Amount <= 0)
        {
            _logger.LogError("TransferBeer validation failed - Amount must be greater than 0. Amount: {Amount}", command.Amount);
            throw new ArgumentException("Amount must be greater than 0.", nameof(command.Amount));
        }

        var senderAccount = await _transferBeer.FindAccountByIdAsync(command.SenderId, cancellationToken);
        if (senderAccount == null)
        {
            _logger.LogWarning("Sender account not found. SenderId: {SenderId}", command.SenderId);
            throw new AccountNotFound(command.SenderId);
        }

        var recipientAccount = await _transferBeer.FindAccountByIdAsync(command.RecipientId, cancellationToken);
        if (recipientAccount == null)
        {
            _logger.LogWarning("Recipient account not found. RecipientId: {RecipientId}", command.RecipientId);
            throw new AccountNotFound(command.RecipientId);
        }

        if (senderAccount.Beer < command.Amount)
        {
            _logger.LogWarning("Insufficient beer. SenderId: {SenderId}, Required: {Required}, Current: {Current}", command.SenderId, command.Amount, senderAccount.Beer);
            throw new InvalidOperationException($"Sender does not have enough beer to transfer.");
        }

        senderAccount.Beer -= command.Amount;
        await _transferBeer.UpdateAccountAsync(senderAccount, cancellationToken);

        recipientAccount.Beer += command.Amount;
        recipientAccount.LifetimeBeer += command.Amount;
        await _transferBeer.UpdateAccountAsync(recipientAccount, cancellationToken);

        await _transferBeer.LogTransactionAsync(senderAccount.Id, recipientAccount.Id, command.Amount, Reason.Donated, cancellationToken);

        _fitzMetrics?.RecordBeerTransfer(command.Amount, Reason.Donated.ToString());
        _fitzMetrics?.RecordTransaction("transfer");

        var model = TransferBeerModel.From(senderAccount, recipientAccount, command.Amount);

        _logger.LogInformation("TransferBeerModel created successfully. SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount}", command.SenderId, command.RecipientId, command.Amount);

        return model;
    }
}
