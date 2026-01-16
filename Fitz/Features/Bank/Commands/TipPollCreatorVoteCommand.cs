using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Models;
using Fitz.Features.Settings;
using Fitz.Variables;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands
{
    public class TipPollCreatorVoteCommand(IServiceScopeFactory scopeFactory, AccountService accountService, SettingsService settingsService, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly AccountService accountService = accountService;
        private readonly SettingsService settingsService = settingsService;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(ulong accountId)
        {
            try
            {
                if (accountId == Users.Fitz)
                {
                    return new Result(false, "Cannot tip Fitz.", null);
                }
                if (accountId == Users.Dodecuplet)
                {
                    return new Result(false, "Cannot tip Dodecuplet.", null);
                }

                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
                var settings = settingsService.GetSettings();

                Account account = accountService.FindAccount(accountId);
                if (account == null)
                {
                    return new Result(false, $"{accountId} did not have an account.", null);
                }

                account.Beer += settings.PollCreatorTip;
                account.LifetimeBeer += settings.PollCreatorTip;
                db.Update(account);
                await db.SaveChangesAsync();
                
                var logTransactionCommand = new LogTransactionCommand(scopeFactory, botLog);
                await logTransactionCommand.ExecuteAsync(account, account, settings.PollCreatorTip, Reason.PollCreatorTip);

                return new Result(true, $"Tipped poll creator vote bonus to {account.Username}.", account);
            }
            catch (Exception ex)
            {
                return new Result(false, ex.Message, null);
            }
        }
    }
}
