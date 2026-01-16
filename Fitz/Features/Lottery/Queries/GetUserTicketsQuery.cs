using Fitz.Core.Contexts;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Lottery.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Fitz.Features.Lottery.Queries
{
    public class GetUserTicketsQuery(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public Result Execute(Account account)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var getCurrentLotteryQuery = new GetCurrentLotteryQuery(scopeFactory);
                var lottery = getCurrentLotteryQuery.Execute();

                if (lottery == null)
                {
                    return new Result(false, "No active lottery found.", null);
                }

                List<Ticket> userTickets = db.Ticket.Where(x => x.AccountId == account.Id && x.Drawing == lottery.Id).ToList();
                return new Result(true, $"Got {userTickets.Count} ticket(s) for {account.Username}.", userTickets);
            }
            catch (System.Exception ex)
            {
                return new Result(false, "Failed to get user tickets.", ex);
            }
        }
    }
}
