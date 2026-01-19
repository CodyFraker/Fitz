using Fitz.Database;
using Fitz.Database.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Fitz.Features.Lottery.Queries
{
    public class GetCurrentLotteryQuery(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public Database.Entities.LotteryEntity Execute()
        {
            using var scope = scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

            return db.Drawing.Where(x => x.CurrentLottery == true).FirstOrDefault();
        }
    }
}
