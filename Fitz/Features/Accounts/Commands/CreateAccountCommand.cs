using DSharpPlus.Entities;
using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Accounts.Queries;
using Fitz.Variables.Emojis;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Commands
{
    public class CreateAccountCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(DiscordUser user)
        {
            var findAccountQuery = new FindAccountQuery(scopeFactory);
            if (findAccountQuery.Execute(user.Id) != null)
            {
                return new Result(true, "You already have an account.", findAccountQuery.Execute(user.Id));
            }

            var account = new Account
            {
                Id = user.Id,
                Username = user.Username,
                Beer = 0,
                LifetimeBeer = 0,
                safeBalance = 128,
                Favorability = 50,
                CreatedDate = DateTime.Now,
                LastSeenDate = DateTime.Now,
                LastActivityDate = DateTime.Now,
                Deactivated = false,
                subscribeToLottery = false,
                SubscribeTickets = 1,
            };

            try
            {
                using var scope = scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                db.Accounts.Add(account);
                await db.SaveChangesAsync();

                Log.Debug($"Added new account to Database: {user.Username} | {user.Id}");
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Add, $"Created a new account for: {user.Username} | {user.Id}");
                return new Result(true, "Account created successfully.", account);
            }
            catch (Exception e)
            {
                this.botLog.Information(LogConsoleSettings.AccountLog, AccountEmojis.Warning, $"Error creating a new account for: {user.Username} | {user.Id}");
                Log.Error(e, $"Couldn't add new account! {user.Username} | {user.Id}");
                return new Result(false, "Failed to create account.", account);
            }
        }
    }
}
