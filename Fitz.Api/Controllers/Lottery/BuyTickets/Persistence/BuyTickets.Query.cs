using Fitz.Api.Controllers.Lottery.BuyTickets.Domain;
using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Fitz.Api.Controllers.Lottery.BuyTickets.Persistence;

public class BuyTickets(IDbContextFactory<BotContext> contextFactory, ILogger<BuyTickets> logger) : IBuyTickets
{
    private readonly IDbContextFactory<BotContext> _contextFactory = contextFactory;
    private readonly ILogger<BuyTickets> _logger = logger;

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

    public async Task<SettingsEntity?> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting lottery settings");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await context.Settings
            .FirstOrDefaultAsync(cancellationToken);

        if (settings != null)
        {
            _logger.LogInformation("Settings found. MaxTickets: {MaxTickets}, TicketCost: {TicketCost}", settings.MaxTickets, settings.TicketCost);
        }
        else
        {
            _logger.LogWarning("Settings not found");
        }

        return settings;
    }

    public async Task<LotteryEntity?> GetCurrentLotteryAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting current lottery");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var lottery = await context.Drawing
            .Where(x => x.CurrentLottery == true)
            .FirstOrDefaultAsync(cancellationToken);

        if (lottery != null)
        {
            _logger.LogInformation("Current lottery found. LotteryId: {LotteryId}", lottery.Id);
        }
        else
        {
            _logger.LogInformation("No current lottery found");
        }

        return lottery;
    }

    public async Task<List<TicketEntity>> GetUserTicketsAsync(ulong userId, int lotteryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user tickets. UserId: {UserId}, LotteryId: {LotteryId}", userId, lotteryId);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var tickets = await context.Ticket
            .Where(x => x.AccountId == userId && x.Drawing == lotteryId)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("User tickets found. UserId: {UserId}, LotteryId: {LotteryId}, Count: {Count}", userId, lotteryId, tickets.Count);

        return tickets;
    }

    public async Task<List<TicketEntity>> CreateTicketsAsync(ulong userId, int lotteryId, int count, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating tickets. UserId: {UserId}, LotteryId: {LotteryId}, Count: {Count}", userId, lotteryId, count);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var existingTickets = await context.Ticket
            .Where(x => x.AccountId == userId && x.Drawing == lotteryId)
            .Select(x => x.Number)
            .ToListAsync(cancellationToken);

        var tickets = new List<TicketEntity>();

        for (int i = 0; i < count; i++)
        {
            int uniqueTicketNumber = GenerateUniqueTicketNumber(existingTickets);
            existingTickets.Add(uniqueTicketNumber);

            var ticket = new TicketEntity
            {
                AccountId = userId,
                Drawing = lotteryId,
                Number = uniqueTicketNumber,
                Timestamp = DateTime.UtcNow
            };

            tickets.Add(ticket);
        }

        await context.Ticket.AddRangeAsync(tickets, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tickets created successfully. UserId: {UserId}, LotteryId: {LotteryId}, Count: {Count}", userId, lotteryId, count);

        return tickets;
    }

    public async Task AddToPoolAsync(int lotteryId, int amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding to pool. LotteryId: {LotteryId}, Amount: {Amount}", lotteryId, amount);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var lottery = await context.Drawing
            .Where(x => x.Id == lotteryId)
            .FirstOrDefaultAsync(cancellationToken);

        if (lottery == null)
        {
            _logger.LogWarning("Lottery not found. LotteryId: {LotteryId}", lotteryId);
            throw new InvalidOperationException($"Lottery not found: {lotteryId}");
        }

        lottery.Pool = (lottery.Pool ?? 0) + amount;
        context.Update(lottery);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added to pool successfully. LotteryId: {LotteryId}, Amount: {Amount}, NewPool: {NewPool}", lotteryId, amount, lottery.Pool);
    }

    private int GenerateUniqueTicketNumber(List<int> existingNumbers)
    {
        int maxAttempts = 100;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int ticketNumber = GenerateTicketNumber();
            if (!existingNumbers.Contains(ticketNumber))
            {
                return ticketNumber;
            }
        }

        throw new InvalidOperationException("Failed to generate unique ticket number after multiple attempts");
    }

    private int GenerateTicketNumber()
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] data = new byte[4];
            int ticketNumber = 0;
            for (int i = 0; i < 4; i++)
            {
                rng.GetBytes(data);
                ticketNumber = BitConverter.ToInt32(data, 0);
                ticketNumber = Math.Abs(ticketNumber);
                ticketNumber %= 1000;
            }
            if (ticketNumber >= 0 && ticketNumber <= 1000)
            {
                return ticketNumber;
            }
            else
            {
                return GenerateTicketNumber();
            }
        }
    }
}
