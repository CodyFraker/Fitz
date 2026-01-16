using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Commands
{
    public class CreateFitzAccountCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync()
        {
            var account = new Account
            {
                Id = Users.Fitz,
                Username = "Fitz",
                LifetimeBeer = 128,
                Beer = 128,
                Favorability = 100,
                CreatedDate = DateTime.Now,
                LastSeenDate = DateTime.Now,
                LastActivityDate = DateTime.Now,
                subscribeToLottery = false,
                SubscribeTickets = 1,
            };

            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
                db.Accounts.Add(account);
                await db.SaveChangesAsync();
                Log.Debug($"Created an account for Fitz");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Add, $"Fitz account not found in database. Created an account for Fitz.");
                return new Result(true, "Account created successfully.", account);
            }
            catch (Exception e)
            {
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Failed to create an account for Fitz. {e.StackTrace}");
                Log.Error(e, $"Failed to create an account for Fitz.");
                return new Result(false, "Failed to create account.", account);
            }
        }
    }
}
