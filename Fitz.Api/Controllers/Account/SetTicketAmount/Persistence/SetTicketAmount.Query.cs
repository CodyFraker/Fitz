using Fitz.Api.Controllers.Account.SetTicketAmount.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Account.SetTicketAmount.Persistence;

public class SetTicketAmount(IDbContextFactory<BotContext> contextFactory, ILogger<SetTicketAmount> logger) : ISetTicketAmount
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<SetTicketAmount> _logger = logger;

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
        _logger.LogInformation("Updating account. UserId: {UserId}, SubscribeTickets: {SubscribeTickets}", account.Id, account.SubscribeTickets);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Accounts.Update(account);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account updated successfully. UserId: {UserId}, SubscribeTickets: {SubscribeTickets}", account.Id, account.SubscribeTickets);
    }
}
