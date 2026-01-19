using DSharpPlus.Entities;
using Fitz.Database;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Database.Entities;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Queries;
using Fitz.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    public sealed class AccountService(IServiceScopeFactory scopeFactory, BotLog botLog, FitzMetrics? fitzMetrics = null)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;
        private readonly FitzMetrics? fitzMetrics = fitzMetrics;

        public async Task<Result> CreateAccountAsync(DiscordUser user)
        {
            var command = new CreateAccountCommand(scopeFactory, botLog, fitzMetrics);
            var result = await command.ExecuteAsync(user);
            
            if (result.Success)
            {
                var accounts = this.QueryAccounts();
                var activeAccounts = accounts.Where(a => !a.Deactivated).Count();
                fitzMetrics?.SetAccountsActive(activeAccounts);
            }
            
            return result;
        }

        public async Task<Result> CreateAccountAsync(ulong userId, string username)
        {
            var findAccountQuery = new FindAccountQuery(scopeFactory);
            if (findAccountQuery.Execute(userId) != null)
            {
                return new Result(true, "Account already exists.", findAccountQuery.Execute(userId));
            }

            var account = new Account
            {
                Id = userId,
                Username = username,
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

                fitzMetrics?.RecordAccountCreated();
                
                var accounts = this.QueryAccounts();
                var activeAccounts = accounts.Where(a => !a.Deactivated).Count();
                fitzMetrics?.SetAccountsActive(activeAccounts);
                
                return new Result(true, "Account created successfully.", account);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Couldn't add new account! {username} | {userId}");
                return new Result(false, "Failed to create account.", account);
            }
        }

        public async Task<Result> SetSafeBalanceAsync(Account account, int safeBalance)
        {
            var command = new SetSafeBalanceCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, safeBalance);
        }

        public async Task<Result> SetSafeBalanceAsync(ulong userId, int safeBalance)
        {
            var findAccountQuery = new FindAccountQuery(scopeFactory);
            var account = findAccountQuery.Execute(userId);
            return await SetSafeBalanceAsync(account, safeBalance);
        }

        public async Task<Result> SetLotterySubscribe(Account account, bool subscribe)
        {
            var command = new SetLotterySubscribeCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, subscribe);
        }

        public async Task<Result> SetTicketAmountAsync(Account account, int amount)
        {
            var command = new SetTicketAmountCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, amount);
        }

        public async Task<Result> SetFavorabilityAsync(Account account, int newFavorability)
        {
            var command = new SetFavorabilityCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, newFavorability);
        }

        public async Task<Result> SetUsernameAsync(Account account, string username)
        {
            var command = new SetUsernameCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, username);
        }

        public async Task<Result> SetDeactivatedAsync(Account account, bool deactivated)
        {
            var command = new SetDeactivatedCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, deactivated);
        }

        public List<Account> QueryAccounts()
        {
            var query = new QueryAccountsQuery(scopeFactory);
            return query.Execute();
        }

        public List<Account> GetLotterySubscribers()
        {
            var query = new GetLotterySubscribersQuery(scopeFactory);
            return query.Execute();
        }

        public Account FindAccount(ulong id)
        {
            var query = new FindAccountQuery(scopeFactory);
            return query.Execute(id);
        }

        public Account FindAccount(DiscordUser user)
        {
            var query = new FindAccountQuery(scopeFactory);
            return query.Execute(user);
        }

        public DSharpPlus.Entities.DiscordEmbed AccountHelpEmbed(DSharpPlus.DiscordClient dClient)
        {
            var query = new GetAccountHelpEmbedQuery();
            return query.Execute(dClient);
        }
    }
}
