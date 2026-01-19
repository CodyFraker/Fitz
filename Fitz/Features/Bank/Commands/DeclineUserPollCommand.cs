using Fitz.Database;
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
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands
{
    public class DeclineUserPollCommand(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService, BotLog botLog, FitzMetrics? fitzMetrics = null)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly SettingsService settingsService = settingsService;
        private readonly BotLog botLog = botLog;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public async Task<Result> ExecuteAsync(ulong userId)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
                var settings = settingsService.GetSettings();

                AccountEntity account = accountService.FindAccount(userId);
                if (account == null)
                {
                    return new Result(false, $"{userId} did not have an account.", null);
                }

                var favorabilityService = new FavorabilityService(scopeFactory, accountService, settingsService);
                var transferToFitzCommand = new TransferToFitzCommand(scopeFactory, accountService, settingsService, favorabilityService, botLog, fitzMetrics);
                await transferToFitzCommand.ExecuteAsync(account.Id, settings.PollDeclinedPenalty, Reason.PollDeclined);

                fitzMetrics?.RecordTransaction("poll_declined_penalty");

                return new Result(true, $"Deducted poll declined penalty from {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
