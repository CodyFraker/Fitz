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
    public class AwardPollVoteCommand(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService, BotLog botLog)
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

                account.Beer += settings.PollVote;
                account.LifetimeBeer += settings.PollVote;
                db.Update(account);
                await db.SaveChangesAsync();
                
                var logTransactionCommand = new LogTransactionCommand(scopeFactory, botLog);
                await logTransactionCommand.ExecuteAsync(account, account, settings.PollVote, Reason.PollVote);

                return new Result(true, $"Awarded poll vote bonus to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
