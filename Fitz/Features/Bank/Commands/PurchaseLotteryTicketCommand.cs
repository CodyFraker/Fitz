using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using Fitz.Database.Entities;
using Fitz.Database.Entities;
using Fitz.Features.Favorability;
using Fitz.Features.Settings;
using Fitz.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands
{
    public class PurchaseLotteryTicketCommand(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService, BotLog botLog, FitzMetrics? fitzMetrics = null)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly SettingsService settingsService = settingsService;
        private readonly BotLog botLog = botLog;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public async Task<Result> ExecuteAsync(Account user, int amount)
        {
            try
            {
                var favorabilityService = new FavorabilityService(scopeFactory, accountService, settingsService);
                var transferToFitzCommand = new TransferToFitzCommand(scopeFactory, accountService, settingsService, favorabilityService, botLog, fitzMetrics);
                await transferToFitzCommand.ExecuteAsync(user.Id, amount, Reason.Lotto);
                return new Result(true, $"Purchased {amount} lottery ticket(s).", user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to purchase {amount} lottery ticket(s) for {user.Username} | {user.Id}");
                return new Result(false, $"Failed to purchase {amount} tickets for {user.Username} | {user.Id}", null);
            }
        }
    }
}
