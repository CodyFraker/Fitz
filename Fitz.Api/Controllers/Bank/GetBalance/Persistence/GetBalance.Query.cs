using Fitz.Api.Controllers.Bank.GetBalance.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Api.Controllers.Bank.GetBalance.Persistence;

public class GetBalance(IDbContextFactory<BotContext> contextFactory, ILogger<GetBalance> logger) : IGetBalance
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<GetBalance> _logger = logger;

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

    public async Task<AccountEntity> CreateAccountAsync(ulong userId, string username, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating account. UserId: {UserId}, Username: {Username}", userId, username);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var account = new AccountEntity
        {
            Id = userId,
            Username = username,
            Beer = 0,
            LifetimeBeer = 0,
            safeBalance = 128,
            Favorability = 50,
            CreatedDate = DateTime.UtcNow,
            LastSeenDate = DateTime.UtcNow,
            LastActivityDate = DateTime.UtcNow,
            Deactivated = false,
            subscribeToLottery = false,
            SubscribeTickets = 1
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account created successfully. UserId: {UserId}, Username: {Username}", userId, username);

        return account;
    }
}
