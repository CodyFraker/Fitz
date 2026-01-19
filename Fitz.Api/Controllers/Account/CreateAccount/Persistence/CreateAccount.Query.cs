using Fitz.Api.Controllers.Account.CreateAccount.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Account.CreateAccount.Persistence;

public class CreateAccount(IDbContextFactory<BotContext> contextFactory, ILogger<CreateAccount> logger) : ICreateAccount
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<CreateAccount> _logger = logger;

    public async Task Save(CreateAccountModel model, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Saving account to database. AccountId: {AccountId}, Username: {Username}", model.Id, model.Username);
        
        var newAccountEntity = new AccountEntity
        {
            Id = model.Id,
            Username = model.Username,
            Beer = model.Beer,
            LifetimeBeer = model.LifetimeBeer,
            safeBalance = model.SafeBalance,
            Favorability = model.Favorability,
            CreatedDate = model.CreatedOn,
            LastSeenDate = model.LastSeenDate,
            LastActivityDate = model.LastActivityDate,
            subscribeToLottery = model.SubscribedToLottery,
            SubscribeTickets = model.SubscribeTickets,
            Deactivated = model.Deactivated,
        };

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        await context.Accounts.AddAsync(newAccountEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Account saved to database successfully. AccountId: {AccountId}, Username: {Username}", model.Id, model.Username);
    }

    public async Task<AccountEntity?> FindByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding account by ID. AccountId: {AccountId}", id);
        
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var account = await context.Accounts
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (account != null)
        {
            _logger.LogInformation("Account found by ID. AccountId: {AccountId}, Username: {Username}", id, account.Username);
        }
        else
        {
            _logger.LogInformation("Account not found by ID. AccountId: {AccountId}", id);
        }
        
        return account;
    }
}
