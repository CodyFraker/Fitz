using Fitz.Api.Controllers.Bank.TransferBeer.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Transaction = Fitz.Database.Entities.Transaction;

namespace Fitz.Api.Controllers.Bank.TransferBeer.Persistence;

public class TransferBeer(IDbContextFactory<BotContext> contextFactory, ILogger<TransferBeer> logger) : ITransferBeer
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<TransferBeer> _logger = logger;

    public async Task<AccountEntity?> FindAccountByIdAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding account by ID. UserId: {UserId}", userId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var account = await context.Accounts
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (account != null)
        {
            _logger.LogInformation("Account found. UserId: {UserId}, Username: {Username}", userId, account.Username);
        }
        else
        {
            _logger.LogInformation("Account not found. UserId: {UserId}", userId);
        }

        return account;
    }

    public async Task UpdateAccountAsync(AccountEntity account, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating account. UserId: {UserId}, Beer: {Beer}", account.Id, account.Beer);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Accounts.Update(account);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account updated successfully. UserId: {UserId}, Beer: {Beer}", account.Id, account.Beer);
    }

    public async Task LogTransactionAsync(ulong senderId, ulong recipientId, int amount, Reason reason, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Logging transaction. SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount}, Reason: {Reason}", senderId, recipientId, amount, reason);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var transaction = new Transaction
        {
            Sender = senderId,
            Recipient = recipientId,
            Amount = amount,
            Reason = reason,
            Timestamp = DateTime.UtcNow
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transaction logged successfully. SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount}", senderId, recipientId, amount);
    }
}
