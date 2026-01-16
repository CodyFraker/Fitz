using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Models;
using Fitz.Features.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands
{
    public class UserSubmittedPollPenaltyCommand(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly SettingsService settingsService = settingsService;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(ulong userId)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
                var settings = settingsService.GetSettings();

                Account account = accountService.FindAccount(userId);
                if (account == null)
                {
                    return new Result(false, $"{userId} did not have an account.", null);
                }

                var transferToFitzCommand = new TransferToFitzCommand(scopeFactory, botLog);
                await transferToFitzCommand.ExecuteAsync(account.Id, (settings.PollSubmittedPenalty + settings.PollDeclinedPenalty), Reason.PollSubmitted);

                return new Result(true, $"Deducted poll submitted penalty from {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
