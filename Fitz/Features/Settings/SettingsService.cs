using Fitz.Core.Models;
using Fitz.Features.Settings.Commands;
using Fitz.Features.Settings.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Fitz.Features.Settings
{
    public sealed class SettingsService(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        public async Task<Result> CreateBaseSettings()
        {
            var command = new CreateBaseSettingsCommand(scopeFactory);
            return await command.ExecuteAsync();
        }

        public async Task<Result> SetLotteryDuration(int days)
        {
            var command = new SetLotteryDurationCommand(scopeFactory);
            return await command.ExecuteAsync(days);
        }

        public async Task<Result> SetBaseLotteryPool(int pool)
        {
            var command = new SetBaseLotteryPoolCommand(scopeFactory);
            return await command.ExecuteAsync(pool);
        }

        public async Task<Result> SetLotteryPoolRollover(bool rollover)
        {
            var command = new SetLotteryPoolRolloverCommand(scopeFactory);
            return await command.ExecuteAsync(rollover);
        }

        public async Task<Result> SetTicketCost(int cost)
        {
            var command = new SetTicketCostCommand(scopeFactory);
            return await command.ExecuteAsync(cost);
        }

        public async Task<Result> SetMaxTickets(int maxTickets)
        {
            var command = new SetMaxTicketsCommand(scopeFactory);
            return await command.ExecuteAsync(maxTickets);
        }

        public async Task<Result> SetHappyHourBaseAmount(int amount)
        {
            var command = new SetHappyHourBaseAmountCommand(scopeFactory);
            return await command.ExecuteAsync(amount);
        }

        public async Task<Result> SetAccountCreationBonusAmount(int amount)
        {
            var command = new SetAccountCreationBonusAmountCommand(scopeFactory);
            return await command.ExecuteAsync(amount);
        }

        public async Task<Result> SetRenameBaseCost(int cost)
        {
            var command = new SetRenameBaseCostCommand(scopeFactory);
            return await command.ExecuteAsync(cost);
        }

        public Core.Models.Settings GetSettings()
        {
            var query = new GetSettingsQuery(scopeFactory);
            return query.Execute();
        }
    }
}
