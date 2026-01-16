using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Lottery.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Commands
{
    public class CreateTicketCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(Account account, int totalTickets)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var getCurrentLotteryQuery = new Queries.GetCurrentLotteryQuery(scopeFactory);
                var lottery = getCurrentLotteryQuery.Execute();

                if (lottery == null)
                {
                    return new Result(false, "No active lottery found.", null);
                }

                List<Ticket> tickets = new List<Ticket>();

                for (int i = 0; i < totalTickets; i++)
                {
                    int uniqueTicketNumber = 0;
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        byte[] data = new byte[4];

                        for (int j = 0; j < 4; j++)
                        {
                            rng.GetBytes(data);
                            uniqueTicketNumber = BitConverter.ToInt32(data, 0);
                            uniqueTicketNumber = Math.Abs(uniqueTicketNumber);
                        }
                    }

                    Ticket ticket = new Ticket
                    {
                        AccountId = account.Id,
                        Drawing = lottery.Id,
                        Number = uniqueTicketNumber,
                        Timestamp = DateTime.UtcNow
                    };

                    tickets.Add(ticket);
                }

                db.Ticket.AddRange(tickets);
                await db.SaveChangesAsync();

                Log.Debug($"Created {totalTickets} ticket(s) for {account.Username} | {account.Id}");
                this.botLog.Information(LogConsoleSettings.LotteryLog, Variables.Emojis.LotteryEmojis.Ticket, $"Created {totalTickets} ticket(s) for {account.Username} | {account.Id}");

                return new Result(true, $"Created {totalTickets} ticket(s) for {account.Username}.", tickets);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to create tickets for {account.Username} | {account.Id}");
                return new Result(false, $"Failed to create tickets for {account.Username} | {account.Id}", null);
            }
        }
    }
}
