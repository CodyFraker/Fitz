using Fitz.Database;
using Fitz.Core.Discord;
using Fitz.Database.Entities;
using Fitz.Features.Accounts;
using Fitz.Features.Favorability;
using Fitz.Features.Settings;
using Fitz.Metrics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Fitz.Core.Models;

namespace Fitz.Features.Bank.Commands
{
    public class UserSubmittedPollPenaltyCommand(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService, BotLog botLog, FitzMetrics? fitzMetrics = null)
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
                await transferToFitzCommand.ExecuteAsync(account.Id, (settings.PollSubmittedPenalty + settings.PollDeclinedPenalty), Reason.PollSubmitted);

                fitzMetrics?.RecordTransaction("poll_submitted_penalty");

                return new Result(true, $"Deducted poll submitted penalty from {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
