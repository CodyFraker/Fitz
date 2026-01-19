using Fitz.Api.Controllers.Account.CreateAccount.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Account.CreateAccount.Persistence;

public class CreateAccount(IDbContextFactory<BotContext> contextFactory) : ICreateAccount
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;

    public async Task Save(CreateAccountModel model, CancellationToken cancellationToken = default)
    {
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
    }

    public async Task<AccountEntity?> FindByIdAsync(ulong id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Accounts
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
